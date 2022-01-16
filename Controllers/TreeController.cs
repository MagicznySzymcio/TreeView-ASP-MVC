using Drzewo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Drzewo.Controllers
{
    public class TreeController : Controller
    {
        private readonly TreeDbContext _dbContext;
        public TreeController(TreeDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // Sorting methods: "asc" - ascending, default; "desc" - descending
        public async Task<IActionResult> Index(string sort = "asc")
        {
            // Get sorted tree
            var nodesList = await GetNodesList(sort);

            return View(nodesList);
        }


        [Route("Tree/Add/{pid?}")]
        public async Task<IActionResult> Add(int? pid)
        {
            if (pid == null)
                return NotFound();

            // When not adding new root
            if (pid != 0)
            {
                _dbContext.Nodes.Load();
                var node = await _dbContext.Nodes.FindAsync(pid);

                if (node == null)
                    return NotFound();

                ViewData["Pid"] = node.Id;
                ViewData["Path"] = node.GeneratePath();
            }
            // When adding new root
            else
            {
                ViewData["Pid"] = null;
                ViewData["Path"] = "/";
            }

            return View();
        }

        [HttpPost]
        [Route("Tree/Add/{pid?}")]
        public async Task<IActionResult> Add([Bind("Name", "ParentId")] Node node)
        {
            if (ModelState.IsValid)
            {
                // Set new "Depth" for node
                if (node.ParentId == null)
                    node.Depth = 1;
                else
                {
                    var parentDepth = await _dbContext.Nodes.FindAsync(node.ParentId);
                    if (parentDepth == null)
                        return NotFound();
                    node.Depth = parentDepth.Depth + 1;
                }
                await _dbContext.AddAsync(node);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Get new ViewData if modelState is not valid
            if (node.ParentId != 0)
            {
                await _dbContext.Nodes.LoadAsync();
                node = await _dbContext.Nodes.FindAsync(node.ParentId);

                if (node == null)
                    return NotFound();

                ViewData["Pid"] = node.Id;
                ViewData["Path"] = node.GeneratePath();
            }
            // When adding new root
            else
            {
                ViewData["Pid"] = null;
                ViewData["Path"] = "/";
            }

            return View(node);
        }


        [Route("Tree/Delete/{id?}")]
        public async Task<IActionResult> Delete(int? id)
        {
            // Don't allow to remove root
            if (id == null || id == 0)
                return NotFound();

            var node = await _dbContext.Nodes.FindAsync(id);

            if (node == null)
                return NotFound();

            return View(node);
        }

        [HttpPost, ActionName("Delete")]
        [Route("Tree/Delete/{id?}")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id == 0)
                return NotFound();

            var node = await _dbContext.Nodes.FindAsync(id);

            if (node == null)
                return NotFound();

            await _dbContext.Nodes.LoadAsync();
            // Delete Node with children
            RemoveChildren(node);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [Route("Tree/Edit/{id?}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            var node = await _dbContext.Nodes.FindAsync(id);

            if (node == null)
            {
                return NotFound();
            }

            return View(node);
        }

        [HttpPost]
        [Route("Tree/Edit/{id?}")]
        public async Task<IActionResult> Edit(int id, string name)
        {
            var node = await _dbContext.Nodes.FindAsync(id);

            if (node == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid && name.Length <= 24)
            {
                node.Name = name;
                _dbContext.Update(node);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(node);
        }


        [Route("Tree/Move/{id?}")]
        public async Task<IActionResult> Move(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var node = await _dbContext.Nodes.FindAsync(id);

            if (node == null)
                return NotFound();

            // Make new SelectList, add root to SelectList, and remove all children
            var selectList = await _dbContext.Nodes.Where(n => n.Id != id).ToListAsync();
            await _dbContext.Nodes.LoadAsync();
            selectList = RemoveChildrenFromList(selectList, node);
            selectList.Insert(0, new Node
            {
                Id = 0,
                Name = "/.",
                Depth = 0
            });

            ViewData["Parents"] = new SelectList(selectList, "Id", "Name");

            return View(node);
        }

        [HttpPost]
        [Route("Tree/Move/{id?}")]
        public async Task<IActionResult> Move(int id, int parentId)
        {
            // Check if parent id is not same or one of children of current node
            var childrenIds = GetChildrenIds(id, new List<int>());

            foreach (int childId in childrenIds)
            {
                if (parentId == childId)
                    return NotFound();
            }

            var node = await _dbContext.Nodes.FindAsync(id);
            if (node == null)
                return NotFound();

            if (parentId != 0)
            {
                var parentNode = await _dbContext.Nodes.FindAsync(parentId);
                if (parentNode == null)
                    return NotFound();
            }

            if (ModelState.IsValid)
            {
                // When moving root
                if (parentId == 0)
                {
                    node.ParentId = null;
                    node.Parent = null;
                    node.Depth = 1;
                }
                else
                    node.ParentId = parentId;

                _dbContext.Update(node);
                await _dbContext.SaveChangesAsync();

                await _dbContext.Nodes.LoadAsync();
                // Update self and children
                UpdateChildrenDepth(node);
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Get new ViewData if modelstate is not Valid
            var selectList = await _dbContext.Nodes.Where(n => n.Id != id).ToListAsync();
            selectList = RemoveChildrenFromList(selectList, node);
            selectList.Insert(0, new Node
            {
                Id = 0,
                Name = "/.",
                Depth = 0
            });

            ViewData["Parents"] = new SelectList(selectList, "Id", "Name");

            return View(node);
        }

        // Update "Depth" for node and all of his children
        private void UpdateChildrenDepth(Node node)
        {
            // Update Depth
            if (node.Parent != null)
                node.Depth = node.Parent.Depth + 1;
            else
                node.Depth = 1;

            _dbContext.Update(node);

            // Repeat for all children
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    UpdateChildrenDepth(child);
                }
            }
        }

        // Recursive removes children from database
        private void RemoveChildren(Node node)
        {
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    RemoveChildren(child);
                }
            }

            _dbContext.Remove(node);
        }

        // Remove children from list
        private List<Node> RemoveChildrenFromList(List<Node> children, Node node)
        {
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    children = RemoveChildrenFromList(children, child);
                    children.Remove(child);
                }
            }
            else
                children.Remove(node);

            return children;
        }

        // Get full tree
        private async Task<List<Node>> GetNodesList(string order)
        {
            List<Node> nodesList = new List<Node>();
            await _dbContext.Nodes.LoadAsync();
            var rootNodes = await _dbContext.Nodes.Where(n => n.Depth == 1).ToListAsync();

            // Descending
            if (order == "desc")
            {
                rootNodes = rootNodes.OrderByDescending(n => n.Name).ToList();
                foreach (var node in rootNodes)
                {
                    // Add all roots with children
                    nodesList.Add(node);
                    node.ToListDesc(nodesList);
                }
            }
            // Ascending
            else
            {
                rootNodes = rootNodes.OrderBy(n => n.Name).ToList();
                foreach (var node in rootNodes)
                {
                    nodesList.Add(node);
                    node.ToListAsc(nodesList);
                }
            }

            return nodesList;
        }

        // Get parent and children ID
        private List<int> GetChildrenIds(int id, List<int> childrenIds)
        {
            var node = _dbContext.Nodes.AsNoTracking().Where(n => n.Id == id).Include(n => n.Children).FirstOrDefault();
            if (node.Children.Count != 0)
            {
                foreach (var child in node.Children)
                {
                    childrenIds = GetChildrenIds(child.Id, childrenIds);
                }
                childrenIds.Add(id);
            }
            else
                childrenIds.Add(id);

            return childrenIds;
        }

    }
}

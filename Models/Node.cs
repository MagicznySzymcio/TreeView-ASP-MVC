using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace Drzewo.Models
{
    public class Node
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(24)]
        public string Name { get; set; }

        [Required]
        public int Depth { get; set; }

        [Display(Name = "Parent")]
        public int? ParentId { get; set; }
        public virtual Node Parent { get; set; }
        public virtual ICollection<Node> Children { get; set; }

        // Recursive get all children
        public List<Node> ToList(List<Node> nodeList)
        {
            if (Children != null)
            {
                foreach (var child in Children)
                {
                    nodeList.Add(child);
                    nodeList = child.ToList(nodeList);
                }
                return nodeList;
            }
            else
            {
                return nodeList;
            }
        }

        // Get all children in ascending order
        public List<Node> ToListAsc(List<Node> nodeList)
        {
            if (Children != null)
            {
                Children = Children.OrderBy(n => n.Name).ToList();
                foreach (var child in Children)
                {
                    nodeList.Add(child);
                    nodeList = child.ToList(nodeList);
                }
                return nodeList;
            }
            else
            {
                return nodeList;
            }
        }

        // Get all children in descending order
        public List<Node> ToListDesc(List<Node> nodeList)
        {
            if (Children != null)
            {
                Children = Children.OrderByDescending(n => n.Name).ToList();
                foreach (var child in Children)
                {
                    nodeList.Add(child);
                    nodeList = child.ToList(nodeList);
                }
                return nodeList;
            }
            else
            {
                return nodeList;
            }
        }

        // Generate path to file
        public string GeneratePath()
        {
            string path = Name + "/";
            while (Parent != null)
            {
                path = Parent.Name + "/" + path;
                Parent = Parent.Parent;
            }
            return "/" + path;
        }
    }
}

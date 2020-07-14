using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EfSqlite.Entities
{
    public class Quote
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; } = default;

        [Required]
        [MinLength(3)]
        public string Text { get; set; }
        
    }
}
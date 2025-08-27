using ASP.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ASP.Data.Configurations
{
    public class GroupConfiguration : IEntityTypeConfiguration<ProductGroup>
    {
        public void Configure(EntityTypeBuilder<Entities.ProductGroup> builder)
        {
            builder
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder
                .HasOne(g => g.ParentGroup)
                .WithMany()
                .HasForeignKey(g => g.ParentId);

            builder
                .HasMany(g => g.Products)
                .WithOne(p => p.Group)
                .HasForeignKey(p => p.GroupId);
        }
    }
}

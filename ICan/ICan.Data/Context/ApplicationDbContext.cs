using ICan.Common.Domain;
using ICan.Common.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ICan.Data.Context
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public virtual DbSet<OptCampaign> OptCampaign { get; set; }
		public virtual DbSet<OptBlockType> OptBlockType { get; set; }
		public virtual DbSet<OptMovePaper> OptMovePaper { get; set; }
		public virtual DbSet<OptPayment> OptPayment { get; set; }
		public virtual DbSet<OptSpringOrderIncoming> OptSpringOrderincoming { get; set; }
		public virtual DbSet<OptSpringOrder> OptSpringOrder { get; set; }
		public virtual DbSet<OptNumberOfTurns> OptNumberOfTurns { get; set; }
		public virtual DbSet<OptSpring> OptSpring { get; set; }
		public virtual DbSet<OptApplicationUserShopRelation> OptApplicationUserShopRelation { get; set; }
		public virtual DbSet<OptGluePadIncoming> OptGluePadIncoming { get; set; }
		public virtual DbSet<OptTag> OptTag { get; set; }
		public virtual DbSet<OptSemiproductProductRelation> OptSemiproductProductRelation { get; set; }
		public virtual DbSet<OptMarketplace> OptMarketplace { get; set; }
		public virtual DbSet<OptTypeOfPaper> OptTypeOfPaper { get; set; }
		public virtual DbSet<OptPaperOrderIncoming> OptPaperOrderIncoming { get; set; }
		public virtual DbSet<OptEvent> OptEvent { get; set; }
		public virtual DbSet<OptReportCriteria> OptReportCriteria { get; set; }
		public virtual DbSet<OptReportCriteriaGroup> OptReportCriteriaGroups { get; set; }
		public virtual DbSet<OptAssembly> OptAssembly { get; set; }
		public virtual DbSet<OptAssemblySemiproduct> OptAssemblySemiproduct { get; set; }
		public virtual DbSet<OptCounterparty> OptCounterparty { get; set; }
		public virtual DbSet<OptPaperOrderRole> OptPaperOrderRole { get; set; }
		public virtual DbSet<OptNotchOrder> OptNotchOrder { get; set; }
		public virtual DbSet<OptNotchOrderItem> OptNotchOrderItem { get; set; }
		public virtual DbSet<OptNotchOrderSticker> OptNotchOrderSticker { get; set; }
		public virtual DbSet<OptNotchOrderIncoming> OptNotchOrderIncoming { get; set; }
		public virtual DbSet<OptNotchOrderIncomingItem> OptNotchOrderIncomingItem { get; set; }
		public virtual DbSet<OptPaperOrder> OptPaperOrder { get; set; }
		public virtual DbSet<OptPrintOrder> OptPrintOrder { get; set; }
		public virtual DbSet<OptPrintOrderSemiproduct> OptPrintOrderSemiproduct { get; set; }
		public virtual DbSet<OptPrintOrderIncoming> OptPrintOrderIncoming { get; set; }
		public virtual DbSet<OptPrintOrderIncomingItem> OptPrintOrderIncomingItem { get; set; }
		public virtual DbSet<OptPrintOrderPaper> OptPrintOrderPaper { get; set; }
		public virtual DbSet<OptSemiproductType> OptSemiproductType { get; set; }
		public virtual DbSet<OptPaper> OptPaper { get; set; }
		public virtual DbSet<OptMaterial> OptMaterial { get; set; }
		public virtual DbSet<OptFormat> OptFormat { get; set; }
		public virtual DbSet<OptSemiproduct> OptSemiproduct { get; set; }

		public virtual DbSet<OptOrderPhoto> OptOrderPhotos { get; set; }

		public virtual DbSet<OptOrder> OptOrder { get; set; }

		public virtual DbSet<OptOrderpayment> OptOrderpayment { get; set; }

		public virtual DbSet<OptOrderproduct> OptOrderproduct { get; set; }

		public virtual DbSet<OptKitproduct> OptKitproduct { get; set; }

		public virtual DbSet<OptOrderstatus> OptOrderstatus { get; set; }

		public virtual DbSet<OptProduct> OptProduct { get; set; }

		public virtual DbSet<OptProductkind> OptProductkind { get; set; }

		public virtual DbSet<OptProductprice> OptProductprice { get; set; }

		public virtual DbSet<OptDiscount> OptDiscount { get; set; }

		public virtual DbSet<OptProductseries> OptProductseries { get; set; }

		public virtual DbSet<ApplicationUser> OptUser { get; set; }

		public virtual DbSet<OptReport> OptReport { get; set; }

		public virtual DbSet<OptOrderSizeDiscount> OptOrderSizeDiscount { get; set; }
		public virtual DbSet<OptReportitem> OptReportitem { get; set; }

		public virtual DbSet<OptReportkind> OptReportkind { get; set; }

		public virtual DbSet<OptShop> OptShop { get; set; }

		public virtual DbSet<OptShopName> OptShopName { get; set; }

		public virtual DbSet<OptWarehouse> OptWarehouse { get; set; }

		public virtual DbSet<OptWarehouseItem> OptWarehouseItem { get; set; }
		public virtual DbSet<OptWarehouseType> OptWarehouseType { get; set; }
		public virtual DbSet<OptSemiproductWarehouse> OptSemiProductWarehouse { get; set; }

		public virtual DbSet<OptSemiproductWarehouseItem> OptSemiproductWarehouseItem { get; set; }

		public virtual DbSet<OptWarehouseActionType> OptWarehouseActionType { get; set; }

		public virtual DbSet<OptRequisites> OptRequisites { get; set; }

		public virtual DbSet<OptGlobalSetting> OptGlobalSetting { get; set; }
		public virtual DbSet<OptWarehouseJournal> OptWarehouseJournal { get; set; }
		public virtual DbSet<OptSemiproductPaper> OptSemiproductPaper { get; set; }
		public virtual DbSet<OptUtmStatistics> OptUtmStatistics { get; set; }
		public virtual DbSet<OptPrintOrderPayment> OptPrintOrderPayment { get; set; }
		public virtual DbSet<OptUpdPayment> OptUpdPayment { get; set; }
		public virtual DbSet<OptWbWarehouse> OptWbWarehouse { get; set; }
		public virtual DbSet<OptWbOrder> OptWbOrder { get; set; }
		public virtual DbSet<OptWbSale> OptWbSale { get; set; }
		public virtual DbSet<OptProductTag> OptProductTag { get; set; }
		public virtual DbSet<OptProductImage> OptProductImage { get; set; }
		public virtual DbSet<OptImage> OptImage { get; set; }
		public virtual DbSet<OptMarketplaceProduct> OptMarketplaceProduct { get; set; }
		public virtual DbSet<OptMarketplaceProductUrl> OptMarketplaceProductUrl { get; set; }
		public virtual DbSet<OptSite> OptSite { get; set; }
		public virtual DbSet<OptSiteFilter> OptSiteFilter { get; set; }
		public virtual DbSet<OptSiteFilterProduct> OptSiteFilterProduct { get; set; }
		public virtual DbSet<OptCountry> OptCountry { get; set; }

		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
			: base(options)
		{
		}

		protected override void OnModelCreating(ModelBuilder builder)
		{
			base.OnModelCreating(builder);
			// Customize the ASP.NET Identity model and override the defaults if needed.
			// For example, you can rename the ASP.NET Identity table names and more.
			// Add your customizations after calling base.OnModelCreating(builder);

			builder.Entity<OptApplicationUserShopRelation>(entity =>
			{
				entity.HasKey(e => new { e.ShopId, e.UserId });

				entity.ToTable("opt_applicationusershoprelation");
				entity.HasOne(x => x.Shop).WithMany(x => x.ApplicationUserShopRelations).HasForeignKey(x => x.ShopId);
				entity.HasOne(x => x.User).WithMany(x => x.ApplicationUserShopRelations).HasForeignKey(x => x.UserId);
			});

			builder.Entity<OptGluePadIncoming>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.ToTable("opt_gluepadincoming");
			});

			builder.Entity<OptSemiproductProductRelation>(entity =>
			{
				entity.HasKey(e => new { e.SemiproductId, e.ProductId });

				entity.ToTable("opt_semiproductproductrelation");

				entity.HasOne(x => x.Product).WithMany(x => x.RelatedSemiproducts).HasForeignKey(x => x.ProductId);
				entity.HasOne(x => x.Semiproduct).WithMany(x => x.RelatedProducts).HasForeignKey(x => x.SemiproductId);
			});

			builder.Entity<OptMarketplace>(entity =>
			{
				entity.HasKey(e => e.MarketplaceId);

				entity.ToTable("opt_marketplace");

				entity.Property(e => e.Name).HasColumnType("varchar(50)");
				entity.HasOne(x => x.Site)
				.WithMany(x => x.Marketplaces)
				.HasForeignKey(x => x.SiteId);
			});

			builder.Entity<OptTag>(entity =>
			{
				entity.HasKey(e => e.TagId);

				entity.ToTable("opt_tag");

				entity.Property(e => e.TagName).HasColumnType("varchar(50)");
			});

			builder.Entity<OptProductseries>(entity =>
			{
				entity.HasKey(e => e.ProductSeriesId);

				entity.ToTable("opt_productseries");

				entity.Property(e => e.ProductSeriesId)
					.HasColumnName("ProductSeriesID");
				///.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});

			builder.Entity<OptEvent>(entity =>
			{
				entity.HasKey(e => e.EventId);

				entity.ToTable("opt_event");

				entity.Property(e => e.EventId)
					.HasColumnName("EventID");
				//.HasColumnType("int(11)");

				entity.Property(e => e.Description).HasMaxLength(500);

				entity.Property(e => e.DiscountPercent).HasColumnType("float");

				entity.Property(e => e.EndDate).HasColumnType("datetime");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.StartDate).HasColumnType("datetime");
			});

			builder.Entity<OptAssembly>(entity =>
			{
				entity.HasKey(e => e.AssemblyId);

				entity.ToTable("opt_assembly");
				entity.Property(e => e.AssemblyId)
					.HasColumnName("AssemblyID");
				//	.HasColumnType("int(20)");


				entity.HasOne(d => d.Product)
					.WithMany(p => p.Assemblies)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_Assembly_ProductID");

				entity.HasMany(x => x.AssemblySemiproducts)
					.WithOne(x => x.Assembly)
					.HasForeignKey(x => x.AssemblySemiproductId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Assembly_AssemblySemiproduct");

				entity.HasOne<OptWarehouse>(d => d.Warehouse)
					.WithOne(p => p.Assembly)
					.HasForeignKey<OptAssembly>(e => e.WarehouseId)
					.OnDelete(DeleteBehavior.SetNull)
					.HasConstraintName("Opt_Assembly_WarehouseID");

			});

			builder.Entity<OptCounterparty>(entity =>
			{
				entity.HasKey(e => e.CounterpartyId);

				entity.ToTable("opt_counterparty");

				entity.Property(e => e.CounterpartyId)
					.HasColumnName("CounterpartyID");
				//.HasColumnType("int(20)");

				entity.Property(e => e.Name).HasMaxLength(200);

				entity.Property(e => e.Consignee).HasMaxLength(200);

				entity.Property(e => e.Inn).HasMaxLength(12);

				entity.Property(e => e.Enabled)
					.HasColumnType("tinyint(1)");

				entity.Property(e => e.PaperOrderRoleId)
					.HasColumnType("int");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);

				entity.HasOne(d => d.PaperOrderRole)
					.WithMany(p => p.CounterParties)
					.HasForeignKey(d => d.PaperOrderRoleId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_CounterParty_PaperOrderRoleID");

			});

			builder.Entity<OptPaperOrder>(entity =>
			{
				entity.HasKey(e => e.PaperOrderId);  //+

				entity.ToTable("opt_paperorder");

				entity.Property(e => e.PaperOrderId)  //+
					.HasColumnName("PaperOrderID");
				//	.HasColumnType("int(20)");

				entity.Property(e => e.FormatId)
				.HasColumnName("FormatID")  //+
				.IsRequired()
				.HasColumnType("int");

				entity.Property(e => e.PaperId)  //+
				.HasColumnName("PaperID")
					.IsRequired()
				.HasColumnType("int");

				entity.Property(e => e.SupplierCounterPartyId)
				   .HasColumnName("SupplierCounterPartyID")
					.IsRequired()
				.HasColumnType("int");

				entity.Property(e => e.RecieverCounterPartyId)
				   .HasColumnName("RecieverCounterPartyID")
				.HasColumnType("int");

				entity.HasOne(d => d.Format)
					.WithMany(p => p.PaperOrders)
					.HasForeignKey(d => d.FormatId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PaperOrder_FormatID")
					.IsRequired();

				entity.HasOne(d => d.Paper)
					.WithMany(p => p.PaperOrders)
					.HasForeignKey(d => d.PaperId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PaperOrder_PaperID")
					.IsRequired();

				entity.HasOne(d => d.SupplierCounterParty)
					.WithMany(p => p.PaperOrderSuppliers)
					.HasForeignKey(d => d.SupplierCounterPartyId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PaperOrder_SupplierCounterPartyID")
					.IsRequired();

				entity.HasOne(d => d.RecieverCounterParty)
					.WithMany(p => p.PaperOrderRecievers)
					.HasForeignKey(d => d.RecieverCounterPartyId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PaperOrder_RecieverCounterPartyID");

				entity.HasMany(d => d.PrintOrderPapers)
					.WithOne(p => p.PaperOrder)
					.HasForeignKey(d => d.PaperOrderId)
					.OnDelete(DeleteBehavior.Restrict);

			});

			builder.Entity<OptPrintOrderPaper>(entity =>
			{
				entity.HasKey(e => e.PrintOrderPaperId);

				entity.ToTable("opt_printorderpaper");

				entity.Property(e => e.PrintOrderPaperId)
					.HasColumnName("PrintOrderPaperID");

				entity.Property(e => e.PaperOrderId)
					.HasColumnName("PaperOrderID")
					.HasColumnType("int(20)");

				entity.HasOne(d => d.PaperOrder)
					.WithMany(p => p.PrintOrderPapers)
					.HasForeignKey(d => d.PaperOrderId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PrintOrderPaper_PaperOrderID");

				entity.HasOne(d => d.PrintOrder)
					.WithMany(p => p.PrintOrderPapers)
					.HasForeignKey(d => d.PrintOrderId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_PrintOrderPaper_PrintOrderID");
			});

			builder.Entity<OptPrintOrder>(entity =>
			{
				entity.HasKey(e => e.PrintOrderId);

				entity.ToTable("opt_printorder");

				entity.Property(e => e.PrintOrderId)
					.HasColumnName("PrintOrderID");
				//	.HasColumnType("int(20)");

				entity.Property(e => e.PrintingHouseOrderNum)
				.HasColumnName("PrintingHouseOrderNum")
				.HasColumnType("varchar(20)");

				entity.Property(e => e.OrderDate)
				   .HasColumnName("OrderDate")
				   .IsRequired()
				   .HasColumnType("datetime");

				entity.Property(e => e.Printing)
				   .HasColumnName("Printing")
				   .IsRequired()
				   .HasColumnType("int(11)");

				entity.Property(e => e.ConfirmPrint)
				   .HasColumnName("ConfirmPrint");
			});


			builder.Entity<OptPrintOrderSemiproduct>(entity =>
			{
				entity.HasKey(e => e.PrintOrderSemiproductId);

				entity.ToTable("opt_printordersemiproduct");

				entity.Property(e => e.PrintOrderSemiproductId)
					.HasColumnName("PrintOrderSemiproductID");
				//.HasColumnType("int(20)");

				entity.Property(e => e.PrintOrderId)
					.HasColumnName("PrintOrderID")
					.HasColumnType("int(20)");

				entity.Property(e => e.SemiproductId)
					.HasColumnName("SemiProductID");
				//.HasColumnType("int");

				entity.HasOne(d => d.PrintOrder)
					.WithMany(p => p.PrintOrderSemiproducts)
					.HasForeignKey(d => d.PrintOrderId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_PrintOrderSemiproduct_PrintOrderID");

				entity.HasOne(d => d.SemiProduct)
					.WithMany(p => p.PrintOrderSemiproducts)
					.HasForeignKey(d => d.SemiproductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PrintOrderSemiproduct_SemiproductID");

				entity.HasMany(s => s.AssemblySemiproducts)
					.WithOne(s => s.PrintOrderSemiproduct)
					.HasForeignKey(s => s.PrintOrderSemiproductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PrintOrderSemiproduct_AssemblySemiproduct");
			});

			builder.Entity<OptPrintOrderIncoming>(entity =>
			{
				entity.HasKey(e => e.PrintOrderIncomingId);

				entity.ToTable("opt_printorderincoming");

				entity.Property(e => e.PrintOrderIncomingId)
					.HasColumnName("PrintOrderIncomingID");
				//.HasColumnType("int(20)");

				entity.Property(e => e.PrintOrderId)
					.HasColumnName("PrintOrderID")
					.HasColumnType("int(20)");

				entity.Property(e => e.IncomingDate)
					.HasColumnName("IncomingDate")
					.HasColumnType("datetime");

				entity.HasOne(d => d.PrintOrder)
					.WithMany(p => p.PrintOrderIncomings)
					.HasForeignKey(d => d.PrintOrderId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PrintOrderIncomin_PrintOrderID");
			});

			builder.Entity<OptPrintOrderIncomingItem>(entity =>
			{
				entity.HasKey(e => e.PrintOrderIncomingItemId);

				entity.ToTable("opt_printorderincomingitem");

				entity.Property(e => e.PrintOrderIncomingItemId)
					.HasColumnName("PrintOrderIncomingItemID");
				//		.HasColumnType("int(20)");

				entity.Property(e => e.PrintOrderIncomingId)
					.HasColumnName("PrintOrderIncomingID")
					.HasColumnType("int(20)");

				entity.Property(e => e.PrintOrderSemiproductId)
					.HasColumnName("PrintOrderSemiproductID")
					.HasColumnType("int(20)");

				entity.Property(e => e.Amount)
					.HasColumnName("Amount")
					.HasColumnType("int(11)");

				entity.HasOne(d => d.PrintOrderIncoming)
					.WithMany(p => p.PrintOrderIncomingItems)
					.HasForeignKey(d => d.PrintOrderIncomingId)
					.OnDelete(DeleteBehavior.Cascade)
				 	.HasConstraintName("Opt_PrintOrderIncomingItem_PrintOrderIncomingID");

				entity.HasOne(d => d.PrintOrderSemiproduct)
					.WithMany(p => p.PrintOrderIncomingItems)
					.HasForeignKey(d => d.PrintOrderSemiproductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_PrintOrderIncomingItem_PrintOrderSemiproductID");
			});

			builder.Entity<OptSemiproductType>(entity =>
			{
				entity.HasKey(e => e.SemiproductTypeId);

				entity.ToTable("opt_semiproducttype");

				entity.Property(e => e.SemiproductTypeId)
					.HasColumnName("SemiproductTypeID");
				///.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});

			builder.Entity<OptPaper>(entity =>
			{
				entity.HasKey(e => e.PaperId);

				entity.ToTable("opt_paper");

				entity.Property(e => e.PaperId)
					.HasColumnName("PaperID");
				//	.HasColumnType("int(20)");

				entity.Property(e => e.Description).HasMaxLength(500);

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);

				entity.HasOne(x => x.TypeOfPaper)
						.WithMany(x => x.Papers)
						.HasForeignKey(x => x.TypeOfPaperId)
						.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_Paper_TypeOfPaperId");
			});

			builder.Entity<OptPaperOrderRole>(entity =>
			{
				entity.HasKey(e => e.PaperOrderRoleId);

				entity.ToTable("opt_paperorderrole");

				entity.Property(e => e.PaperOrderRoleId)
					.HasColumnName("PaperOrderRoleID");
				//.HasColumnType("int(20)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});

			builder.Entity<OptFormat>(entity =>
			{
				entity.HasKey(e => e.FormatId);

				entity.ToTable("opt_format");

				entity.Property(e => e.FormatId)
					.HasColumnName("FormatID");
				//	.HasColumnType("int(20)");

				entity.Property(e => e.Description)
					.HasMaxLength(500);

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});


			builder.Entity<OptSemiproduct>(entity =>
			{
				entity.ToTable("opt_semiproduct");
				entity.HasKey(e => e.SemiproductId);

				entity.Property(e => e.SemiproductId)
					.HasColumnName("SemiproductID");

				entity.Property(e => e.ProductId)
				.HasColumnName("ProductID")
				.IsRequired()
				.HasColumnType("int(11)");

				entity.Property(e => e.SemiproductTypeId)
				.HasColumnName("SemiproductTypeID")
				.IsRequired()
				.HasColumnType("int(11)");

				//entity.Property(e => e.FormatId)
				//.HasColumnName("FormatID")
				//.HasColumnType("int(20)");

				entity.HasOne(d => d.Product)
					.WithMany(p => p.Semiproducts)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_SemiProduct_ProductID");

				entity.HasOne(d => d.Format)
					.WithMany(p => p.Semiproducts)
					.HasForeignKey(d => d.FormatId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_SemiProduct_FormatID");

				entity.HasOne(d => d.SemiproductType)
					.WithMany(p => p.Semiproducts)
					.HasForeignKey(d => d.SemiproductTypeId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_SemiProduct_SemiproductTypeID");

				entity.HasMany(x => x.RelatedProducts).WithOne(x => x.Semiproduct).HasForeignKey(x => x.SemiproductId);
			});

			builder.Entity<OptOrderSizeDiscount>(entity =>
			{
				entity.HasKey(e => e.OrdersizeDiscountId);

				entity.ToTable("opt_ordersizediscount");

				entity.Property(e => e.OrdersizeDiscountId)
					.HasColumnName("OrderSizeDiscountID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.From).HasColumnType("float");
				entity.Property(e => e.To).HasColumnType("float");

				entity.Property(e => e.DiscountPercent).HasColumnType("double");

				entity.Property(e => e.DateEnd).HasColumnType("datetime");
				entity.Property(e => e.DateStart).HasColumnType("datetime");

				entity.Property(e => e.ClientType).HasColumnType("int(11)");

			});

			builder.Entity<OptDiscount>(entity =>
			{
				entity.HasKey(e => e.DiscountId);

				entity.ToTable("opt_discount");

				entity.Property(e => e.DiscountId)
					.HasColumnName("DiscountID");
				//	.HasColumnType("BIGINT(20)");

				entity.Property(e => e.Description).HasMaxLength(500);

				entity.Property(e => e.Value).HasColumnType("float");
			});

			builder.Entity<OptOrderPhoto>(entity =>
			{
				entity.HasKey(e => e.OrderPhotoId);

				entity.ToTable("opt_orderphoto");

				entity.Property(e => e.OrderPhotoId)
					.HasColumnName("OrderPhotoID")
					.IsRequired();
				//.HasColumnType("int(11)");

				entity.Property(e => e.OrderId)
				.HasColumnType("char(36)").IsRequired();

				entity.HasOne(d => d.Order)
					.WithMany(p => p.Photos)
					.HasForeignKey(d => d.OrderId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_OrderPhoto_Order");
			});

			builder.Entity<OptOrder>(entity =>
			{
				entity.HasKey(e => e.OrderId);

				entity.ToTable("opt_order");

				entity.HasIndex(e => e.OrderStatusId)
					.HasName("Opt_ProductStatus");

				entity.Property(e => e.OrderId)
					.HasColumnName("OrderID")
					.HasColumnType("char(36)");

				entity.Property(e => e.ShortOrderId)
				.HasColumnName("ShortOrderID")
				.HasColumnType("int")
				.ValueGeneratedOnAdd();

				entity.Property(e => e.ClientId)
					.IsRequired()
					.HasColumnName("ClientID")
					.HasMaxLength(767);

				entity.Property(e => e.OrderDate).HasColumnType("datetime");

				entity.Property(e => e.OrderStatusId)
					.HasColumnName("OrderStatusID")
					.HasColumnType("int(11)");

				entity.Property(e => e.EventId)
				.HasColumnName("EventID")
				.HasColumnType("int(11)");

				entity.Property(e => e.RequisitesId)
				.HasColumnName("RequisitesID")
				.HasColumnType("int(11)");

				entity.HasOne(d => d.Event)
					.WithMany(p => p.OptOrder)
					.HasForeignKey(d => d.EventId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_Event");

				entity.HasOne(d => d.Discount)
					.WithMany(p => p.OptOrder)
					.HasForeignKey(d => d.PersonalDiscountId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("FK_Order_DiscountID");

				entity.HasOne(d => d.OrderStatus)
					.WithMany(p => p.OptOrder)
					.HasForeignKey(d => d.OrderStatusId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_ProductStatus");

				entity.HasOne(d => d.Requisites)
					.WithMany(p => p.OptOrders)
					.HasForeignKey(d => d.RequisitesId)
					.OnDelete(DeleteBehavior.SetNull)
					.HasConstraintName("FK_Order_RequisitesID");

				entity.HasOne(d => d.Shop)
					.WithMany(p => p.Orders)
					.HasForeignKey(d => d.ShopId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("FK_Order_ShopID");
			});

			builder.Entity<OptOrderpayment>(entity =>
			{
				entity.HasKey(e => e.OrderPaymentId);

				entity.ToTable("opt_orderpayment");

				entity.HasIndex(e => e.OrderId)
					.HasName("Opt_OrderPayment");

				entity.Property(e => e.OrderPaymentId)
					.HasColumnName("OrderPaymentID");

				entity.Property(e => e.Amount).HasColumnType("double");
				entity.Property(e => e.OrderPaymentDate).HasColumnType("datetime");

				entity.Property(e => e.OrderId)
					.HasColumnName("OrderID")
					.HasColumnType("char(36)");

				entity.HasOne(d => d.Order)
					.WithMany(p => p.OptOrderpayments)
					.HasForeignKey(d => d.OrderId)
					.HasConstraintName("Opt_OrderPayment_OrderId")
					.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptPrintOrderPayment>(entity =>
			{
				entity.HasKey(e => e.PrintOrderPaymentId);

				entity.ToTable("opt_printorderpayment");

				entity.Property(e => e.PrintOrderPaymentId)
					.HasColumnName("PrintOrderPaymentID");

				entity.HasOne(d => d.PrintOrder)
					.WithMany(p => p.PrintOrderPayments)
					.HasForeignKey(d => d.PrintOrderId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptUpdPayment>(entity =>
			{
				entity.HasKey(e => e.UpdPaymentId);

				entity.ToTable("opt_updpayment");

				entity.Property(e => e.UpdPaymentId)
					.HasColumnName("UpdPaymentID");

				entity.HasOne(d => d.Shop)
					.WithMany(p => p.UpdPayments)
					.HasForeignKey(d => d.ShopId)
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptOrderproduct>(entity =>
			{
				entity.HasKey(e => e.OrderProductId);

				entity.ToTable("opt_orderproduct");

				entity.HasIndex(e => e.OrderId)
					.HasName("Opt_OrderProduct");

				entity.HasIndex(e => e.ProductId)
					.HasName("Opt_OrderProductItself");

				entity.Property(e => e.OrderProductId)
					.HasColumnName("OrderProductID");
				//		.HasColumnType("int(11)");

				entity.Property(e => e.Amount).HasColumnType("int(11)");

				entity.Property(e => e.OrderId)
					.HasColumnName("OrderID")
					.HasColumnType("char(36)");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID")
					.HasColumnType("int(11)");

				entity.HasOne(d => d.Order)
					.WithMany(p => p.OptOrderproducts)
					.HasForeignKey(d => d.OrderId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_OrderProduct_OrderId");

				entity.HasOne(d => d.ProductPrice)
					.WithMany(p => p.OptOrderproduct)
					.HasForeignKey(d => d.ProductPriceId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_Orderproduct_ProductPriceId");

				entity.HasOne(d => d.Product)
					.WithMany(p => p.OptOrderproduct)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_OrderProduct_ProductId");
			});

			builder.Entity<OptKitproduct>(entity =>
			{
				entity.HasKey(e => e.KitProductId);

				entity.ToTable("opt_kitproduct");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.MainProductId)
					.HasColumnName("MainProductID")
					.HasColumnType("int(11)");

				entity.Property(e => e.OrderNum).HasColumnType("int(11)");

				entity.HasOne(d => d.MainProduct)
					.WithMany(p => p.KitProducts)
					.HasForeignKey(d => d.MainProductId);


				//entity.HasOne(d => d.MainProduct)
				//	.WithMany(p => p.MainProduct)
				//	.HasForeignKey(d => d.MainProductId)
				//	.OnDelete(DeleteBehavior.Restrict)
				//	.HasConstraintName("Opt_KitProduct_MainProductID");
			});

			builder.Entity<OptWarehouseActionType>(entity =>
			{
				entity.HasKey(e => e.WarehouseActionTypeId);

				entity.ToTable("opt_WarehouseActionType");

				entity.Property(e => e.WarehouseActionTypeId)
					.HasColumnName("WarehouseActionTypeID");
				//.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.HasColumnType("string");
			});

			builder.Entity<OptWarehouse>(entity =>
			{
				entity.HasKey(e => e.WarehouseId);

				entity.ToTable("opt_warehouse");

				entity.Property(e => e.DateAdd)
						.HasColumnType("datetime");

				entity.Property(e => e.Comment)
						.HasColumnType("VARCHAR(200)");

				entity.HasOne(d => d.WarehouseActionType)
					.WithMany(p => p.Warehouses)
					.HasForeignKey(d => d.WarehouseActionTypeId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_Warehouse_WarehouseActionTypeID");

				entity.HasOne(d => d.WarehouseType)
					.WithMany(p => p.Warehouses)
					.HasForeignKey(d => d.WarehouseTypeId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_Warehouse_WarehouseTypeID");
			});

			builder.Entity<OptWarehouseItem>(entity =>
			{
				entity.HasKey(e => e.WarehouseItemId);

				entity.ToTable("opt_warehouseitem");

				entity.Property(e => e.WarehouseId)
					.HasColumnName("WarehouseID")
					.HasColumnType("int(11)");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID")
					.HasColumnType("int(11)");

				entity.Property(e => e.Amount)
					.HasColumnType("int(11)");

				entity.HasOne(d => d.Product)
					.WithMany(p => p.OptWarehouseItem)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_WarehouseItemProduct");
			});

			builder.Entity<OptWarehouseType>(entity =>
			{
				entity.HasKey(e => e.WarehouseTypeId);
				entity.Property(e => e.WarehouseTypeId)
					.ValueGeneratedNever();

				entity.Property(e => e.ReadyToUse)
					.HasDefaultValue(false);

				entity.ToTable("opt_warehousetype");

				entity.HasOne(d => d.Counterparty)
					.WithMany(p => p.WarehouseTypes)
					.HasForeignKey(d => d.CounterpartyId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_WarehouseType_CounterpartyID");
			});


			builder.Entity<OptSemiproductWarehouse>(entity =>
			{
				entity.HasKey(e => e.SemiproductWarehouseId);

				entity.ToTable("opt_semiproductwarehouse");

				entity.Property(e => e.Date)
						.HasColumnType("datetime");

				entity.HasOne(d => d.WarehouseActionType)
					.WithMany(p => p.SemiproductWarehouses)
					.HasForeignKey(d => d.WarehouseActionTypeId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_SemiproductWarehouse_WarehouseActionTypeID");
			});

			builder.Entity<OptSemiproductWarehouseItem>(entity =>
			{
				entity.HasKey(e => e.SemiproductWarehouseItemId);

				entity.ToTable("opt_semiproductwarehouseitem");

				entity.Property(e => e.SemiproductWarehouseItemId)
					.HasColumnName("SemiproductWarehouseItemID");

				entity.Property(e => e.SemiproductWarehouseId)
					.HasColumnName("SemiproductWarehouseID")
					.HasColumnType("int(11)");

				entity.Property(e => e.SemiproductId)
					.HasColumnName("SemiproductID")
					.HasColumnType("int(11)");

				entity.Property(e => e.Amount)
					.HasColumnType("int(11)");

				entity.HasOne(d => d.SemiproductWarehouse)
					.WithMany(p => p.SemiproductWarehouseItems)
					.HasForeignKey(d => d.SemiproductWarehouseId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_SemiproductWarehouseItem_SemiproductWarehouseID");

				entity.HasOne(d => d.Semiproduct)
					.WithMany(p => p.SemiproductWarehouseItems)
					.HasForeignKey(d => d.SemiproductId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_SemiproductWarehouseItem_SemiproductID");
			});

			builder.Entity<OptOrderstatus>(entity =>
			{
				entity.HasKey(e => e.OrderStatusId);

				entity.ToTable("opt_orderstatus");

				entity.Property(e => e.OrderStatusId)
					.HasColumnName("OrderStatusID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});

			builder.Entity<ApplicationUser>(entity =>
			{
				entity.HasKey(e => e.Id);

				entity.ToTable("aspnetusers");
			});

			builder.Entity<OptShopName>(entity =>
			{
				entity.HasKey(e => e.ShopNameId);

				entity.ToTable("opt_shopname");

				entity.Property(e => e.ShopNameId)
					.HasColumnName("ShopNameID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.Name);
				entity.Property(e => e.Inn);

				entity.HasOne(d => d.Shop)
				   .WithMany(p => p.ShopNames)
				   .HasForeignKey(d => d.ShopId)
					.HasConstraintName("Opt_ShopName_SHopID");
			});

			builder.Entity<OptProduct>(entity =>
			{
				entity.HasKey(e => e.ProductId);

				entity.ToTable("opt_product");

				entity.HasIndex(e => e.ProductKindId)
					.HasName("Opt_Product_Kind");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.ISBN)
					.HasMaxLength(13);

				entity.Property(e => e.ProductKindId)
					.HasColumnName("ProductKindID")
					.HasColumnType("int(11)");

				entity.Property(e => e.ProductSeriesId)
					.HasColumnName("ProductSeriesID")
					.HasColumnType("int(11)");

				entity.Property(e => e.Weight).HasColumnType("float");
				entity
				.HasOne(d => d.KitPart)
				.WithOne(d => d.Product)
				.HasForeignKey<OptKitproduct>(e => e.ProductId);

				entity.HasOne(d => d.ProductKind)
					.WithMany(p => p.OptProduct)
					.HasForeignKey(d => d.ProductKindId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("Opt_Product_Kind");

				entity.HasMany(x => x.RelatedSemiproducts).WithOne(x => x.Product).HasForeignKey(x => x.ProductId);

				entity.HasOne(d => d.Country)
					.WithMany(p => p.Products)
					.HasForeignKey(d => d.CountryId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_Product_CountryId");
			});

			builder.Entity<OptProductkind>(entity =>
			{
				entity.HasKey(e => e.ProductKindId);

				entity.ToTable("opt_productkind");

				entity.Property(e => e.ProductKindId)
					.HasColumnName("ProductKindID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(100);
			});

			builder.Entity<OptProductprice>(entity =>
			{
				entity.HasKey(e => e.ProductPriceId);

				entity.ToTable("opt_productprice");

				//entity.HasIndex(e => e.ClientId)
				//	.HasName("Opt_aspnetusers");  //!!

				entity.HasIndex(e => e.ProductId)
					.HasName("Opt_Product");

				entity.Property(e => e.ProductPriceId)
					.HasColumnName("ProductPriceID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.Price).HasColumnType("DOUBLE");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID")
					.HasColumnType("int(11)");

				entity.Property(e => e.ProductPriceId)
					.HasColumnName("ProductPriceId")
					.HasColumnType("int(11)");

				entity.Property(e => e.DateStart)
				.HasColumnName("DateStart")
				.IsRequired()
				.HasColumnType("datetime");

				entity.Property(e => e.DateEnd)
					.HasColumnName("DateEnd")
					.HasColumnType("datetime");

				entity.Property(e => e.ProductId)
				.HasColumnName("ProductID")
				.HasColumnType("int(11)");

				entity.HasOne(d => d.Product)
					.WithMany(p => p.ProductPrices)
					.HasForeignKey(d => d.ProductId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("Opt_Product");
			});

			builder.Entity<OptReport>(entity =>
			{
				entity.HasKey(e => e.ReportId);

				entity.ToTable("opt_report");

				entity.HasIndex(e => e.ReportKindId)
					.HasName("Opt_Report_ReportKindID");

				entity.HasIndex(e => e.ShopId)
					.HasName("Opt_Report_ShopID_idx");

				entity.Property(e => e.ReportId)
					.HasColumnName("ReportID");

				entity.Property(e => e.ReportKindId)
					.HasColumnName("ReportKindID")
					.HasColumnType("int(11)");

				entity.Property(e => e.ReportMonth).HasColumnType("int(2)");


				entity.Property(e => e.ReportNum)
				.HasMaxLength(45);

				entity.Property(e => e.ReportYear).HasColumnType("int(4)");

				entity.Property(e => e.ShopId)
					.HasColumnName("ShopID")
					.HasColumnType("int(11)");

				entity.Property(e => e.UploadDate).HasColumnType("datetime");
				entity.Property(e => e.ReportDate).HasColumnType("datetime");

				entity.HasOne(d => d.ReportKind)
					.WithMany(p => p.OptReport)
					.HasForeignKey(d => d.ReportKindId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("Opt_Report_ReportKindID");

				entity.HasOne(d => d.Shop)
					.WithMany(p => p.Report)
					.HasForeignKey(d => d.ShopId)
					.OnDelete(DeleteBehavior.ClientSetNull)
					.HasConstraintName("Opt_Report_ShopID");
			});

			builder.Entity<OptReportitem>(entity =>
			{
				entity.HasKey(e => e.ReportItemId);

				entity.ToTable("opt_reportitem");

				entity.HasIndex(e => e.ProductId)
					.HasName("Opt_ReportItem_ProductID_idx");

				entity.Property(e => e.ReportItemId)
					.HasColumnName("ReportItemID");
				//		.HasColumnType("int(11)");

				entity.Property(e => e.Amount)
					.HasColumnType("int(10)");

				entity.Property(e => e.ProductId)
					.HasColumnName("ProductID")
					.HasColumnType("int(11)");

				entity.Property(e => e.ReportId)
					.HasColumnName("ReportID")
					.HasColumnType("varchar(150)");

				entity.Property(e => e.TotalSum)
					.HasColumnName("TotalSum");

				entity.HasOne(d => d.Report)
					.WithMany(p => p.ReportItems)
					.HasForeignKey(d => d.ReportId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_ReportItem_ReportID");
			});

			builder.Entity<OptReportkind>(entity =>
			{
				entity.HasKey(e => e.ReportKindId);

				entity.ToTable("opt_reportkind");

				entity.Property(e => e.ReportKindId)
					.HasColumnName("ReportKindID");
				//	.HasColumnType("int(11)");

				entity.Property(e => e.Name)
					.IsRequired()
					.HasMaxLength(50);
			});

			builder.Entity<OptRequisites>(entity =>
			{
				entity.HasKey(e => e.RequisitesId);

				entity.ToTable("opt_requisites");

				entity.Property(e => e.RequisitesId)
					.HasColumnName("RequisitesID");

				entity.Property(e => e.Owner)
					.IsRequired()
					.HasMaxLength(100);

				entity.Property(e => e.RequisitesText)
					.IsRequired()
					.HasMaxLength(500);
			});

			builder.Entity<OptGlobalSetting>(entity =>
			{
				entity.HasKey(e => e.GlobalSettingId);

				entity.ToTable("opt_globalsetting");

				entity.Property(e => e.GlobalSettingId)
					.HasColumnName("GlobalSettingID");

				entity.Property(e => e.Content)
					.IsRequired()
					.HasMaxLength(1000);

				entity.Property(e => e.Comment)
					.HasMaxLength(100);
			});

			builder.Entity<OptWarehouseJournal>(entity =>
			{
				entity.HasKey(e => e.WarehousejournalId);

				entity.ToTable("opt_warehousejournal");

				entity.Property(e => e.WarehousejournalId)
					.HasColumnName("WarehousejournalID");

				entity.HasOne(d => d.WarehouseType)
					.WithMany(p => p.WarehouseJournal)
					.HasForeignKey(d => d.WarehouseTypeId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_WarehouseJournal_WarehouseTypeID");
			});

			builder.Entity<OptSemiproductPaper>(entity =>
			{
				entity.HasKey(e => e.SemiproductPaperId);

				entity.ToTable("opt_semiproductpaper");

				entity.Property(e => e.SemiproductPaperId)
					.HasColumnName("SemiproductPaperID");

				entity.Property(e => e.SemiproductId)
					.HasColumnName("SemiproductID");

				entity.Property(e => e.PaperId)
					.HasColumnName("PaperID");

				entity.HasOne(d => d.Paper)
					.WithMany(p => p.SemiproductPapers)
					.HasForeignKey(d => d.PaperId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_SemiProductPaper_PaperID");

				entity.HasOne(d => d.Semiproduct)
					.WithMany(p => p.SemiproductPapers)
					.HasForeignKey(d => d.SemiproductId)
					.OnDelete(DeleteBehavior.Cascade)
					.HasConstraintName("Opt_SemiProductPaper_SemiproductID");
			});

			builder.Entity<OptAssemblySemiproduct>(entity =>
			{
				entity.ToTable("opt_assemblysemiproduct");
				entity.HasKey(e => e.AssemblySemiproductId);
				entity.Property(e => e.AssemblyId).HasColumnName("AssemblyID");
				entity.Property(e => e.PrintOrderSemiproductId).HasColumnName("PrintOrderSemiproductId");

				entity.HasOne(e => e.Assembly)
					.WithMany(e => e.AssemblySemiproducts)
					.HasForeignKey(x => x.AssemblyId)
					.HasConstraintName("Opt_Assembly_AssemblySemiproduct")
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.PrintOrderSemiproduct)
					.WithMany(x => x.AssemblySemiproducts)
					.HasForeignKey(x => x.PrintOrderSemiproductId)
					.HasConstraintName("Opt_PrintOrderSemiproduct_AssemblySemiproduct")
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(x => x.NotchOrder)
					.WithMany(x => x.AssemblyNotchOrders)
					.HasForeignKey(x => x.NotchOrderId)
					.HasConstraintName("Opt_AssemblySemiproduct_NotchOrderID")
					.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptShop>(entity =>
			{
				entity.HasKey(e => e.ShopId);

				entity.ToTable("opt_shop");

				entity.Property(e => e.ShopId)
					.HasColumnName("ShopID");

				entity.Property(e => e.IsMarketPlace)
					.HasColumnType("bit(1)");

				entity.Property(e => e.NonResident)
					.HasColumnType("bit(1)");

				entity.Property(e => e.Name)
					.HasMaxLength(150)
					.IsRequired();

				entity.Property(e => e.Consignee)
					.HasMaxLength(200);

				entity.Property(e => e.ShopUrl)
					.HasMaxLength(500);
			});

			builder.Entity<OptReportCriteriaGroup>(entity =>
			{
				entity.ToTable("opt_reportcriteriagroup");
				entity.HasKey(e => e.GroupId);
				entity.Property(e => e.GroupId).HasColumnName("GroupId");
				entity.Property(e => e.ShopId).HasColumnName("ShopId");
				entity.Property(e => e.Type).HasColumnName("Type");
				entity.Property(e => e.GroupName).HasColumnName("GroupName");
				entity.Property(e => e.IsMain).HasColumnName("IsMain");

				entity.HasOne(x => x.Shop)
					.WithMany(x => x.CriteriaGroups)
					.HasForeignKey(x => x.ShopId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_Shop_ReportCriteriaGroup");

				entity.HasMany(x => x.Criteria)
					.WithOne(x => x.Group)
					.HasForeignKey(x => x.GroupId)
					.OnDelete(DeleteBehavior.Restrict)
					.HasConstraintName("Opt_ReportCriteriaGroup_ReportCriteria");
			});

			builder.Entity<OptReportCriteria>(entity =>
			{
				entity.ToTable("opt_reportcriteria");
				entity.HasKey(e => e.CriteriaId);
				entity.Property(e => e.CriteriaId).HasColumnName("CriteriaId");
				entity.Property(e => e.GroupId).HasColumnName("GroupId");
				entity.Property(e => e.Address).HasColumnName("Address");
				entity.Property(e => e.Information).HasColumnName("Information");

				entity.HasOne(x => x.Group)
					.WithMany(x => x.Criteria)
					.HasForeignKey(x => x.GroupId)
					.HasConstraintName("Opt_Shop_ReportCriteriaGroup");
			});

			builder.Entity<OptUtmStatistics>(entity =>
			{
				entity.ToTable("opt_utmstatistics");
				entity.HasKey(e => e.StatisticsId);
			});

			builder.Entity<OptNotchOrder>(entity =>
			{
				entity.ToTable("opt_notchorder");
				entity.HasKey(e => e.NotchOrderId);
			});

			builder.Entity<OptNotchOrderItem>(entity =>
			{
				entity.ToTable("opt_notchorderitem");
				entity.HasKey(e => e.NotchOrderItemId);

				entity.HasOne(x => x.PrintOrder)
					.WithOne(x => x.NotchOrderItem)
					.HasForeignKey<OptNotchOrderItem>(f => f.PrintOrderId)
					.HasConstraintName("Opt_NotchOrderItem_PrintOrderId")
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne(x => x.NotchOrder)
					.WithMany(x => x.NotchOrderItems)
					.HasForeignKey(f => f.NotchOrderId)
					.HasConstraintName("Opt_NotchOrderItem_NotchOrderId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptNotchOrderSticker>(entity =>
			{
				entity.ToTable("opt_notchordersticker");
				entity.HasKey(e => e.NotchOrderStickerId);

				entity.HasOne(x => x.NotchOrder)
					.WithMany(x => x.NotchOrderStickers)
					.HasConstraintName("Opt_NotchOrderSticker_NotchOrderId")
					.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Semiproduct)
					.WithMany(x => x.NotchOrderStickers)
					.HasConstraintName("Opt_NotchOrderSticker_SemiproductId")
					.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptNotchOrderIncoming>(entity =>
			{
				entity.ToTable("opt_notchorderincoming");
				entity.HasKey(e => e.NotchOrderIncomingId);

				entity.HasOne(x => x.NotchOrder)
					.WithMany(x => x.NotchOrderIncomings)
					.HasForeignKey(x => x.NotchOrderId)
					.HasConstraintName("Opt_NotchOrderIncoming_NotchOrderId")
					.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptNotchOrderIncomingItem>(entity =>
			{
				entity.ToTable("opt_notchorderincomingitem");
				entity.HasKey(e => e.NotchOrderIncomingItemId);

				entity.HasOne(x => x.NotchOrderIncoming)
						.WithMany(x => x.IncomingItems)
						.HasForeignKey(x => x.NotchOrderIncomingId)
						.HasConstraintName("Opt_NotchOredIncomingItem_NotchOrderIncomingId")
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Semiproduct)
						.WithMany(x => x.NotchOrderIncomingItems)
						.HasForeignKey(x => x.SemiproductId)
						.HasConstraintName("Opt_NotchOredIncomingItem_SemiproductId")
						.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptWbWarehouse>(entity =>
			{
				entity.ToTable("opt_wbWarehouse");
				entity.HasKey(e => e.WbWarehouseId);
				entity.Property(e => e.WbWarehouseId)
				.HasColumnName("WbWarehouseID")
				.HasColumnType("bigint");
			});

			builder.Entity<OptWbOrder>(entity =>
			{
				entity.ToTable("opt_wborder");
				entity.HasKey(e => e.WbOrderId);
				entity.Property(e => e.WbOrderId)
					.HasColumnName("WbOrderID")
					.HasColumnType("bigint");
			});

			builder.Entity<OptWbSale>(entity =>
			{
				entity.ToTable("opt_wbsale");
				entity.HasKey(e => e.WbSaleId);
				entity.Property(e => e.WbSaleId)
				.HasColumnName("WbSaleID")
				.HasColumnType("bigint");
			});

			builder.Entity<OptPaperOrderIncoming>(entity =>
			{
				entity.ToTable("opt_paperorderincoming");
				entity.HasKey(e => e.PaperOrderIncomingId);

				entity.HasOne(x => x.PaperOrder)
						.WithMany(x => x.PaperOrderIncomings)
						.HasForeignKey(x => x.PaperOrderId)
						.HasConstraintName("Opt_PaperOrderIncomig_PaperOrderId")
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(d => d.WarehouseType)
					.WithMany(p => p.PaperOrderIncomings)
					.HasForeignKey(d => d.WarehouseTypeId)
					.OnDelete(DeleteBehavior.Restrict)
						.HasConstraintName("Opt_PaperOrderIncomig_WarehouseTypeID");
			});

			builder.Entity<OptTypeOfPaper>(entity =>
			{
				entity.ToTable("opt_typesofpaper");
				entity.HasKey(e => e.TypeOfPaperId);
				entity.Property(p => p.Name).HasColumnType("varchar(100)");
			});


			builder.Entity<OptProductTag>(entity =>
			{
				entity.ToTable("opt_producttag");
				entity.HasKey(e => e.ProductTagId);


				entity.HasOne(x => x.Product)
						.WithMany(x => x.ProductTags)
						.HasForeignKey(x => x.ProductId)
						.HasConstraintName("Opt_ProductTag_ProductId")
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Tag)
						.WithMany(x => x.ProductTags)
						.HasForeignKey(x => x.TagId)
						.HasConstraintName("Opt_ProductTag_TagId")
						.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptProductImage>(entity =>
			{
				entity.ToTable("opt_productimage");
				entity.HasKey(e => e.ProductImageId);

				entity.HasOne(x => x.Product)
						.WithMany(x => x.ProductImages)
						.HasForeignKey(x => x.ProductId)
						.HasConstraintName("Opt_ProductImage_ProductId")
						.OnDelete(DeleteBehavior.Cascade);

			});

			builder.Entity<OptImage>(entity =>
			{
				entity.ToTable("opt_image");
				entity.HasKey(e => e.ImageId);
			});

			builder.Entity<OptMaterial>(entity =>
			{
				entity.ToTable("opt_material");
				entity.HasKey(e => e.MaterialId);
			});

			builder.Entity<OptMarketplaceProduct>(entity =>
			{
				entity.ToTable("opt_marketplaceproduct");
				entity.HasKey(e => e.MarketplaceProductId);

				entity.HasOne(x => x.Product)
						.WithMany(x => x.MarketplaceProducts)
						.HasForeignKey(x => x.ProductId)
						.HasConstraintName("Opt_MarketplaceProduct_ProductId")
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Marketplace)
						.WithMany(x => x.MarketplaceProducts)
						.HasForeignKey(x => x.MarketplaceId)
						.HasConstraintName("Opt_MarketplaceProduct_MarketplaceId")
						.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptMarketplaceProductUrl>(entity =>
			{
				entity.ToTable("opt_marketplaceproducturl");
				entity.HasKey(e => e.MarketplaceProductUrlId);

				entity.HasOne(x => x.MarketplaceProduct)
						.WithMany(x => x.Urls)
						.HasForeignKey(x => x.MarketplaceProductId)
						.HasConstraintName("Opt_MarketplaceProductUrl_MarketplaceProduct")
						.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptSite>(entity =>
			{
				entity.ToTable("opt_site");
				entity.HasKey(e => e.SiteId);
			});

			builder.Entity<OptSiteFilter>(entity =>
			{
				entity.ToTable("opt_sitefilter");
				entity.HasKey(e => e.SiteFilterId);

				entity.HasOne(x => x.Site)
						.WithMany(x => x.SiteFilters)
						.HasForeignKey(x => x.SiteId)
						.HasConstraintName("Opt_SiteFilter_SiteId")
						.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptSiteFilterProduct>(entity =>
			{
				entity.ToTable("opt_sitefilterproduct");
				entity.HasKey(e => e.SiteFilterProductId);

				entity.HasOne(x => x.SiteFilter)
						.WithMany(x => x.Products)
						.HasForeignKey(x => x.SiteFilterId)
						.HasConstraintName("Opt_SiteFilterProduct_SiteFilterId")
						.OnDelete(DeleteBehavior.Cascade);

				entity.HasOne(x => x.Product)
						.WithMany(x => x.SiteFilterProducts)
						.HasForeignKey(x => x.ProductId)
						.HasConstraintName("Opt_SiteFilterProduct_ProductId")
						.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptCountry>(entity =>
			{
				entity.ToTable("opt_country");
				entity.HasKey(e => e.CountryId);


				entity.Property(e => e.Name).HasColumnType("varchar(100)");
				entity.Property(e => e.Prefix).HasColumnType("varchar(10)");
			});

			builder.Entity<OptSpring>(entity =>
			{
				entity.ToTable("opt_spring");
				entity.HasKey(e => e.SpringId);
				entity.Property(e => e.SpringName).HasColumnType("varchar(100)");
			});

			builder.Entity<OptNumberOfTurns>(entity =>
			{
				entity.ToTable("opt_numberOfTurns");
				entity.HasKey(e => e.NumberOfTurnsId);
				entity.Property(e => e.Manufacturer).HasColumnType("varchar(100)");
				entity.HasMany(x => x.Springs)
						.WithOne(x => x.NumberOfTurns)
						.HasForeignKey(x => x.NumberOfTurnsId)
						.HasConstraintName("Opt_NumberOfTurns_NumberOfTurnsId")
						.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptSpringOrder>(entity =>
			{
				entity.ToTable("opt_springOrder");
				entity.HasKey(e => e.SpringOrderId);
				entity.Property(e => e.InvoiceNumber).HasColumnType("varchar(100)");
				entity.Property(e => e.UPDNumber).HasColumnType("varchar(100)");
				entity.Property(e => e.Provider).HasColumnType("varchar(100)");
				entity.Ignore(e => e.Payments);
				entity.HasOne(x => x.Spring)
						.WithMany(x => x.SpringOrders)
						.HasForeignKey(x => x.SpringId)
						.HasConstraintName("Opt_Spring_SpringId")
						.OnDelete(DeleteBehavior.Restrict);
			});

			builder.Entity<OptSpringOrderIncoming>(entity =>
			{
				entity.ToTable("opt_springOrderIncoming");
				entity.HasKey(e => e.SpringOrderIncomingId);

				entity.HasOne(x => x.SpringOrder)
						.WithMany(x => x.SpringOrderIncomings)
						.HasForeignKey(x => x.SpringOrderId)
						.HasConstraintName("Opt_SpringOrder_SpringOrderId")
						.OnDelete(DeleteBehavior.Cascade);
			});

			builder.Entity<OptPayment>(entity =>
			{
				entity.ToTable("opt_payment");
				entity.HasKey(e => e.PaymentId);
			});

			builder.Entity<OptMovePaper>(entity =>
			{
				entity.ToTable("opt_movepaper");
				entity.HasKey(e => e.MovePaperId);
				entity.Property("PrintOrderPaperId").HasColumnName("PrintOrderPaperId");
			});

			builder.Entity<OptBlockType>(entity =>
			{
				entity.ToTable("opt_blocktype");
				entity.HasKey(e => e.BlockTypeId);
			});

			builder.Entity<OptCampaign>(entity =>
			{
				entity.ToTable("opt_campaign");
				entity.HasKey(e => e.CampaignId);
			});
		}
	}
}

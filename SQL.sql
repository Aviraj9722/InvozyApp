
CREATE TABLE [dbo].[tblBusinesses](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BusinessName] [nvarchar](150) NULL,
	[BusinessTypeId] [int] NULL,
	[OwnerName] [nvarchar](100) NULL,
	[GSTIN] [nvarchar](20) NULL,
	[Email] [nvarchar](100) NULL,
	[Address] [nvarchar](250) NULL,
	[City] [nvarchar](50) NULL,
	[Logo] [varchar](max) NULL,
	[IsActive] [bit] NULL,
	[PrinterSizeId] [int] NULL,
	[HideCustomerField] [bit] NULL,
	[HideTableDropDown] [bit] NULL,
	[IsGSTApplicable] [bit] NULL,
	[QRCode] [varchar](max) NULL,
	[MobileNo] [varchar](50) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_tblBusinesses] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblBusinessType]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBusinessType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BusinessTypeName] [nvarchar](100) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_tblBusinessType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCategories]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCategories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[BusinessId] [int] NULL,
 CONSTRAINT [PK_tblCategories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblEnquiry]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblEnquiry](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[EmailId] [nvarchar](100) NULL,
	[MobileNo] [nvarchar](15) NULL,
	[Comments] [nvarchar](max) NULL,
	[Status] [nvarchar](50) NULL,
	[FollowUpOne] [nvarchar](50) NULL,
	[FollowUpTwo] [nvarchar](50) NULL,
	[FollowUpThree] [nvarchar](50) NULL,
	[FollowUpFour] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblEnquiry] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblFeedback]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblFeedback](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](255) NULL,
	[MobileNo] [nvarchar](50) NULL,
	[Feedback] [nvarchar](max) NULL,
	[Ratings] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[BuisnessId] [int] NULL,
 CONSTRAINT [PK_tblFeedback] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblOrderDetails]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblOrderDetails](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OID] [int] NULL,
	[ProductId] [int] NULL,
	[Qty] [decimal](18, 2) NULL,
	[Price] [decimal](18, 2) NULL,
	[Total] [decimal](18, 2) NULL,
	[GSTPercentage] [decimal](10, 2) NULL,
	[GSTAmount] [decimal](10, 2) NULL,
 CONSTRAINT [PK_tblOrderDetails] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblOrderMaster]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblOrderMaster](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerName] [nvarchar](255) NULL,
	[DateOfOrder] [datetime] NULL,
	[TotalAmount] [decimal](10, 2) NULL,
	[GrandTotal] [decimal](18, 2) NULL,
	[GSTTotal] [decimal](10, 2) NULL,
	[PaymentMode] [nvarchar](20) NULL,
	[PaymentStatus] [bit] NULL,
	[Printed] [bit] NULL,
	[UserId] [int] NULL,
	[BuisnessId] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[CustomerMobNo] [nvarchar](15) NULL,
	[TableDetails] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblOrderMaster] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblPrinterSize]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPrinterSize](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PrinterSize] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblPrinterSize] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblProduct]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblProduct](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NULL,
	[RegionalName] [nvarchar](1000) NULL,
	[Code] [nvarchar](50) NULL,
	[GSTPercentage] [decimal](5, 2) NULL,
	[GSTAmount] [decimal](10, 2) NULL,
	[Price] [decimal](18, 2) NULL,
	[CategoryId] [int] NULL,
	[CreatedOn] [datetime] NULL,
	[UserId] [int] NULL,
	[Photo] [varchar](max) NULL,
	[BusinessId] [int] NULL,
	[UoMId] [int] NULL,
	[IsActive] [bit] NULL,
 CONSTRAINT [PK_tblProduct] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblPurchaseOrder_Stock]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblPurchaseOrder_Stock](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BusinessId] [int] NULL,
	[ProductId] [int] NULL,
	[Quantity] [decimal](10, 2) NULL,
	[DateOfPurchase] [datetime] NULL,
	[CreatedBy] [int] NULL,
 CONSTRAINT [PK_tblPurchaseOrder_Stock] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUOM]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUOM](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BusinessId] [int] NULL,
	[UnitName] [nvarchar](20) NULL,
 CONSTRAINT [PK_tblUOM] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUser]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[UserName] [nvarchar](50) NULL,
	[Password] [nvarchar](50) NULL,
	[Role] [nvarchar](50) NULL,
	[MobileNumber] [nvarchar](15) NULL,
	[IsActive] [bit] NULL,
	[CreatedOn] [datetime] NULL,
	[BussinessId] [int] NULL,
	[EmailId] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblUserLicense]    Script Date: 18-11-2025 15:00:25 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblUserLicense](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[LicenseKey] [nvarchar](100) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_tblUserLicense] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[tblBusinesses]  WITH CHECK ADD  CONSTRAINT [FK_tblBusinesses_tblBusinessType] FOREIGN KEY([BusinessTypeId])
REFERENCES [dbo].[tblBusinessType] ([Id])
GO
ALTER TABLE [dbo].[tblBusinesses] CHECK CONSTRAINT [FK_tblBusinesses_tblBusinessType]
GO
ALTER TABLE [dbo].[tblBusinesses]  WITH CHECK ADD  CONSTRAINT [FK_tblBusinesses_tblPrinterSize] FOREIGN KEY([PrinterSizeId])
REFERENCES [dbo].[tblPrinterSize] ([Id])
GO
ALTER TABLE [dbo].[tblBusinesses] CHECK CONSTRAINT [FK_tblBusinesses_tblPrinterSize]
GO
ALTER TABLE [dbo].[tblCategories]  WITH CHECK ADD  CONSTRAINT [FK_tblCategories_tblBusinesses] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblCategories] CHECK CONSTRAINT [FK_tblCategories_tblBusinesses]
GO
ALTER TABLE [dbo].[tblFeedback]  WITH CHECK ADD  CONSTRAINT [FK_tblFeedback_tblBusinesses] FOREIGN KEY([BuisnessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblFeedback] CHECK CONSTRAINT [FK_tblFeedback_tblBusinesses]
GO
ALTER TABLE [dbo].[tblOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_tblOrderDetails_tblOrderMaster] FOREIGN KEY([OID])
REFERENCES [dbo].[tblOrderMaster] ([Id])
GO
ALTER TABLE [dbo].[tblOrderDetails] CHECK CONSTRAINT [FK_tblOrderDetails_tblOrderMaster]
GO
ALTER TABLE [dbo].[tblOrderDetails]  WITH CHECK ADD  CONSTRAINT [FK_tblOrderDetails_tblProduct] FOREIGN KEY([ProductId])
REFERENCES [dbo].[tblProduct] ([Id])
GO
ALTER TABLE [dbo].[tblOrderDetails] CHECK CONSTRAINT [FK_tblOrderDetails_tblProduct]
GO
ALTER TABLE [dbo].[tblOrderMaster]  WITH CHECK ADD  CONSTRAINT [FK_tblOrderMaster_tblBusinesses] FOREIGN KEY([BuisnessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblOrderMaster] CHECK CONSTRAINT [FK_tblOrderMaster_tblBusinesses]
GO
ALTER TABLE [dbo].[tblOrderMaster]  WITH CHECK ADD  CONSTRAINT [FK_tblOrderMaster_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([Id])
GO
ALTER TABLE [dbo].[tblOrderMaster] CHECK CONSTRAINT [FK_tblOrderMaster_tblUser]
GO
ALTER TABLE [dbo].[tblProduct]  WITH CHECK ADD  CONSTRAINT [FK_tblProduct_tblBusinesses] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblProduct] CHECK CONSTRAINT [FK_tblProduct_tblBusinesses]
GO
ALTER TABLE [dbo].[tblProduct]  WITH CHECK ADD  CONSTRAINT [FK_tblProduct_tblCategories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[tblCategories] ([Id])
GO
ALTER TABLE [dbo].[tblProduct] CHECK CONSTRAINT [FK_tblProduct_tblCategories]
GO
ALTER TABLE [dbo].[tblProduct]  WITH CHECK ADD  CONSTRAINT [FK_tblProduct_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([Id])
GO
ALTER TABLE [dbo].[tblProduct] CHECK CONSTRAINT [FK_tblProduct_tblUser]
GO
ALTER TABLE [dbo].[tblPurchaseOrder_Stock]  WITH CHECK ADD  CONSTRAINT [FK_tblPurchaseOrder_Stock_tblBusinesses] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblPurchaseOrder_Stock] CHECK CONSTRAINT [FK_tblPurchaseOrder_Stock_tblBusinesses]
GO
ALTER TABLE [dbo].[tblPurchaseOrder_Stock]  WITH CHECK ADD  CONSTRAINT [FK_tblPurchaseOrder_Stock_tblProduct] FOREIGN KEY([ProductId])
REFERENCES [dbo].[tblProduct] ([Id])
GO
ALTER TABLE [dbo].[tblPurchaseOrder_Stock] CHECK CONSTRAINT [FK_tblPurchaseOrder_Stock_tblProduct]
GO
ALTER TABLE [dbo].[tblUOM]  WITH CHECK ADD  CONSTRAINT [FK_tblUOM_tblBusinesses] FOREIGN KEY([BusinessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblUOM] CHECK CONSTRAINT [FK_tblUOM_tblBusinesses]
GO
ALTER TABLE [dbo].[tblUser]  WITH CHECK ADD  CONSTRAINT [FK_tblUser_tblBusinesses] FOREIGN KEY([BussinessId])
REFERENCES [dbo].[tblBusinesses] ([Id])
GO
ALTER TABLE [dbo].[tblUser] CHECK CONSTRAINT [FK_tblUser_tblBusinesses]
GO
ALTER TABLE [dbo].[tblUserLicense]  WITH CHECK ADD  CONSTRAINT [FK_tblUserLicense_tblUser] FOREIGN KEY([UserId])
REFERENCES [dbo].[tblUser] ([Id])
GO
ALTER TABLE [dbo].[tblUserLicense] CHECK CONSTRAINT [FK_tblUserLicense_tblUser]
GO

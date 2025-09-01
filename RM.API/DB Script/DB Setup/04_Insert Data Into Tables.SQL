USE [KingResearch]
GO
SET IDENTITY_INSERT [dbo].[Expirys] ON 
GO
INSERT [dbo].[Expirys] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'March 2023', NULL, 1, 1, N'0f18873a-88cf-ed11-ab2b-b0c090b35a86', CAST(N'2023-03-31T11:22:33.273' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[Expirys] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'April 2023', NULL, 0, 0, N'1018873a-88cf-ed11-ab2b-b0c090b35a86', CAST(N'2023-03-31T11:22:39.243' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[Expirys] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (3, N'May 2023', NULL, 0, 0, N'66464341-88cf-ed11-ab2b-b0c090b35a86', CAST(N'2023-03-31T11:22:44.577' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Expirys] OFF
GO
SET IDENTITY_INSERT [dbo].[LeadSources] ON 
GO
INSERT [dbo].[LeadSources] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'Web App', N'', 0, 0, N'51e96313-ccb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T13:00:16.650' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[LeadSources] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Social Media', N'', 0, 0, N'b124151b-ccb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T13:00:29.553' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[LeadSources] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (3, N'Whats App', N'', 0, 0, N'b224151b-ccb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T13:00:38.130' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[LeadSources] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (4, N'Instagram', N'', 0, 0, N'c0a45b25-ccb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T13:00:46.793' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[LeadSources] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (5, N'Facebook', N'', 0, 0, N'c1a45b25-ccb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T13:00:52.603' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[LeadSources] OFF
GO
SET IDENTITY_INSERT [dbo].[Menus] ON 
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (1, N'Dashboard', N'', N'/dashboard', 0, 1, 0, 0, N'6bb7ae04-c25b-eb11-977d-00155d53687a', CAST(N'2021-01-21T13:54:03.527' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (2, N'Customers', N'', N'/customers', 0, 2, 0, 0, N'7db447b4-f35b-eb11-b39f-00155d53687a', CAST(N'2021-01-21T19:49:43.470' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (3, N'Add Customers', N'', N'/customers/form', 2, 1, 0, 0, N'7eb447b4-f35b-eb11-b39f-00155d53687a', CAST(N'2021-01-21T19:49:49.470' AS DateTime), NULL, NULL, NULL, 0)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (4, N'View Customers', N'', N'/customers/view', 2, 2, 0, 0, N'874bd8cd-f35b-eb11-b39f-00155d53687a', CAST(N'2021-01-21T19:50:26.360' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (5, N'Customer Services', N'', N'/customers/customerservices/view', 2, 3, 0, 0, N'a6ed9380-9a75-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T11:16:41.750' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (6, N'Add Customer Services', N'', N'/customers/customerservices/form', 2, 4, 0, 0, N'88cdfa9d-9a75-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T11:17:31.080' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (7, N'Settings', N'', N'/settings', 0, 3, 0, 0, N'8e79b4ce-9e75-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T11:47:30.817' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (8, N'Communications', N'', N'/settings/communication/view', 7, 1, 0, 0, N'1ee1a0f4-9e75-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T11:48:34.440' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (9, N'Customer Type', N'', N'/settings/customertype/view', 7, 2, 0, 0, N'6adafa45-a475-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:26:38.397' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (10, N'Lead Sources', N'', N'/settings/source/view', 7, 3, 0, 0, N'c0d9b760-a475-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:27:23.270' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (11, N'Lead Types', N'', N'/settings/type/view', 7, 4, 0, 0, N'd4cf3c77-a475-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:28:01.050' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (12, N'Payment Modes', N'', N'/settings/paymentmodes', 7, 5, 0, 0, N'34dff6a7-a475-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:29:22.817' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (13, N'Segments', N'', N'/settings/segments', 7, 6, 0, 0, N'f4ea50be-a475-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:30:00.297' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (14, N'Packages', N'', N'/packages', 0, 4, 0, 0, N'12487901-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:31:52.970' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (15, N'Service Categories', N's', N'/packages/servicecategories', 14, 1, 0, 0, N'06f8ef27-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:32:57.500' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (16, N'Services', N's', N'/packages/services', 14, 2, 0, 0, N'8ac98f50-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:34:05.657' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (17, N'Service Types', N'u', N'/packages/servicetypes', 14, 3, 0, 0, N'46b37461-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:34:34.017' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (18, N'Leads', N'', N'/leads/view', 0, 5, 0, 0, N'10d28779-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:35:14.393' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (19, N'View Leads', N'', N'/leads/view', 18, 1, 0, 0, N'56388d8b-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:35:44.627' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (20, N'Enquiries', N'', N'/leads/enquiries', 18, 4, 0, 0, N'4c412298-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:36:05.737' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (21, N'Calls', N'', N'/calls ', 0, 6, 0, 0, N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:37:08.657' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (22, N'Call Performance', N'', N'/calls/managecall', 21, 1, 0, 0, N'2a34f4e0-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:38:07.907' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (23, N'Strategies', N'', N'/calls/strategies', 21, 2, 0, 0, N'809ef6f0-a575-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:38:34.773' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (24, N'Stocks', N'', N'/calls/stocks', 21, 3, 0, 0, N'185e6c00-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:39:00.690' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (25, N'Expiry', N'', N'/calls/expiry', 21, 4, 0, 0, N'2674900d-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:39:22.750' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (26, N'Performance Results', N'', N'/calls/performance', 21, 5, 0, 0, N'eadf731d-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:39:49.413' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (27, N'Communication', N'', N'/calls/communicationtemplates', 21, 6, 0, 0, N'221f722f-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:40:19.597' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (28, N'Staff', N'', N'/staff', 0, 7, 0, 0, N'e6f2de48-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:41:02.240' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (29, N'Roles', N'', N'/staff/roles', 28, 1, 0, 0, N'aaa4857c-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:42:28.907' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (30, N'Users', N'', N'/staff/users', 28, 2, 0, 0, N'c852dcfe-a675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:46:07.580' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (31, N'Menus', N'', N'/staff/menus', 28, 3, 0, 0, N'10c30810-a775-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:46:36.390' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (32, N'Role Menu Mapping', N'', N'/staff/rolemapping', 28, 4, 0, 0, N'26695223-a775-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T12:47:08.750' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (33, N'Add Payment Mode', N'', N'/settings/paymentmodes/form', 7, 1, 0, 0, N'44828355-b275-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T14:07:17.413' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (34, N'Lead Spams', N'', N'/leads/spams', 18, 3, 0, 0, N'13125646-659c-eb11-951e-00155d53687a', CAST(N'2021-04-13T20:03:56.060' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (35, N'Allocate Leads', N'', N'/leads/allocateLeads', 18, 2, 0, 0, N'4977af00-709c-eb11-951e-00155d53687a', CAST(N'2021-04-13T21:20:43.670' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (36, N'Partners', N'', N'/partners/view', 0, 1, 0, 0, N'88516d64-5dcf-ec11-80ea-00155d23d79c', CAST(N'2022-05-09T11:30:57.537' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (37, N'KYC', N'', N'/kyc', 0, 2, 0, 0, N'5ed0c22c-612b-ed11-80f1-00155d23d79c', CAST(N'2022-09-03T13:49:49.117' AS DateTime), NULL, NULL, NULL, 1)
GO
INSERT [dbo].[Menus] ([Id], [Name], [Description], [Url], [ParentId], [SortOrder], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [IsLHS]) VALUES (38, N'Payments', N'', N'/purchaseOrder', 0, 9, 0, 0, N'891d6bec-13c3-ed11-bc80-b0c090b35a86', CAST(N'2023-03-15T14:59:46.650' AS DateTime), NULL, NULL, NULL, 1)
GO
SET IDENTITY_INSERT [dbo].[Menus] OFF
GO
SET IDENTITY_INSERT [dbo].[PaymentModes] ON 
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'UPI', N'UPI PhonePay', 0, 0, N'287ea7b9-f4f5-46d1-8305-9a05a5d023cd', CAST(N'2023-03-09T11:11:45.950' AS DateTime), N'1', CAST(N'2023-03-09T11:11:45.950' AS DateTime), N'1')
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'InstaMojo HRY', N'InstaMojo HRY', 0, 0, N'f9ce2ec6-425f-45a1-a3ef-f7dca00741dc', CAST(N'2023-03-12T21:34:18.760' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (3, N'InstaMojo PAN', N'InstaMojo Pankaj', 0, 0, N'a2e17c3f-8d42-4caf-83a8-ce05915f9fb5', CAST(N'2023-03-12T21:34:46.983' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (4, N'8468811500 PP', N'8468811500 Phone Pay', 0, 0, N'a935caa6-efc0-ed11-bc7d-b0c090b35a86', CAST(N'2023-03-12T21:35:05.633' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (5, N'8468811500 GP', N'8468811500 Google Pay', 0, 0, N'aa35caa6-efc0-ed11-bc7d-b0c090b35a86', CAST(N'2023-03-12T21:35:13.983' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[PaymentModes] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (6, N'AXIS Current', N'Axis Bank Current', 0, 0, N'a12a93c3-efc0-ed11-bc7d-b0c090b35a86', CAST(N'2023-03-12T21:35:53.930' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[PaymentModes] OFF
GO
SET IDENTITY_INSERT [dbo].[RoleMenus] ON 
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (11, N'Staff', N'Call Performance', 0, 1, N'249ad45a-e675-eb11-9fe9-00155d53687a', CAST(N'2021-02-23T20:19:40.170' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (44, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'8e79b4ce-9e75-eb11-9fe9-00155d53687a', 0, 0, N'669ca8f0-7885-eb11-94db-00155d53687a', CAST(N'2021-03-15T15:56:45.500' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (45, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', 0, 0, N'18d54dbb-8285-eb11-94db-00155d53687a', CAST(N'2021-03-15T17:06:50.960' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (46, N'Staff', N'Payment Modes', 0, 1, N'8c6a54c9-8285-eb11-94db-00155d53687a', CAST(N'2021-03-15T17:07:14.480' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (47, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'874bd8cd-f35b-eb11-b39f-00155d53687a', 0, 0, N'024b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.607' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (48, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'56388d8b-a575-eb11-9fe9-00155d53687a', 0, 0, N'034b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.617' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (49, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'6bb7ae04-c25b-eb11-977d-00155d53687a', 0, 0, N'044b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.617' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (50, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'd4cf3c77-a475-eb11-9fe9-00155d53687a', 0, 0, N'054b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.617' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (51, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'7eb447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'064b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.627' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (52, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'7db447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'074b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.627' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (53, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'10d28779-a575-eb11-9fe9-00155d53687a', 0, 0, N'084b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.953' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (54, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'c0d9b760-a475-eb11-9fe9-00155d53687a', 0, 0, N'094b97f4-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:19:14.960' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (55, N'Admin', N'View Customers', 0, 1, N'3c557327-bf86-eb11-94db-00155d53687a', CAST(N'2021-03-17T06:51:53.370' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (56, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'6BB7AE04-C25B-EB11-977D-00155D53687A', 0, 0, N'80e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (57, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'7DB447B4-F35B-EB11-B39F-00155D53687A', 0, 0, N'81e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (58, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'7EB447B4-F35B-EB11-B39F-00155D53687A', 0, 0, N'82e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (59, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'874BD8CD-F35B-EB11-B39F-00155D53687A', 0, 0, N'83e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (60, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'A6ED9380-9A75-EB11-9FE9-00155D53687A', 0, 0, N'84e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (61, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'88CDFA9D-9A75-EB11-9FE9-00155D53687A', 0, 0, N'85e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (62, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'8E79B4CE-9E75-EB11-9FE9-00155D53687A', 0, 0, N'86e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (63, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'1EE1A0F4-9E75-EB11-9FE9-00155D53687A', 0, 0, N'87e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (64, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'6ADAFA45-A475-EB11-9FE9-00155D53687A', 0, 0, N'88e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (65, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'C0D9B760-A475-EB11-9FE9-00155D53687A', 0, 0, N'89e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (66, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'D4CF3C77-A475-EB11-9FE9-00155D53687A', 0, 0, N'8ae1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (67, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'34DFF6A7-A475-EB11-9FE9-00155D53687A', 0, 0, N'8be1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (68, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'F4EA50BE-A475-EB11-9FE9-00155D53687A', 0, 0, N'8ce1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (69, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'12487901-A575-EB11-9FE9-00155D53687A', 0, 0, N'8de1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (70, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'06F8EF27-A575-EB11-9FE9-00155D53687A', 0, 0, N'8ee1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (71, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'8AC98F50-A575-EB11-9FE9-00155D53687A', 0, 0, N'8fe1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (72, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'46B37461-A575-EB11-9FE9-00155D53687A', 0, 0, N'90e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (73, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'10D28779-A575-EB11-9FE9-00155D53687A', 0, 0, N'91e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (74, N'Admin', N'View Leads', 0, 1, N'92e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (75, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'4C412298-A575-EB11-9FE9-00155D53687A', 0, 0, N'93e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (76, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'0A5FA3BD-A575-EB11-9FE9-00155D53687A', 0, 0, N'94e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (77, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'2A34F4E0-A575-EB11-9FE9-00155D53687A', 0, 0, N'95e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (78, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'809EF6F0-A575-EB11-9FE9-00155D53687A', 0, 0, N'96e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (79, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'185E6C00-A675-EB11-9FE9-00155D53687A', 0, 0, N'97e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (80, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'2674900D-A675-EB11-9FE9-00155D53687A', 0, 0, N'98e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (81, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'EADF731D-A675-EB11-9FE9-00155D53687A', 0, 0, N'99e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (82, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'221F722F-A675-EB11-9FE9-00155D53687A', 0, 0, N'9ae1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (83, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'E6F2DE48-A675-EB11-9FE9-00155D53687A', 0, 0, N'9be1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (84, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'AAA4857C-A675-EB11-9FE9-00155D53687A', 0, 0, N'9ce1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (85, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'C852DCFE-A675-EB11-9FE9-00155D53687A', 0, 0, N'9de1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (86, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'10C30810-A775-EB11-9FE9-00155D53687A', 0, 0, N'9ee1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (87, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'26695223-A775-EB11-9FE9-00155D53687A', 0, 0, N'9fe1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (88, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'44828355-B275-EB11-9FE9-00155D53687A', 0, 0, N'a0e1f2eb-c386-eb11-94db-00155d53687a', CAST(N'2021-03-17T07:26:01.023' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (89, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'4c412298-a575-eb11-9fe9-00155d53687a', 0, 0, N'343dd4fe-648c-eb11-94db-00155d53687a', CAST(N'2021-03-24T11:21:37.473' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (90, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'88cdfa9d-9a75-eb11-9fe9-00155d53687a', 0, 0, N'9d3e0bdb-8b8c-eb11-94db-00155d53687a', CAST(N'2021-03-24T15:59:47.823' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (91, N'dc99302c-9276-eb11-9fe9-00155d53687a', N'2a34f4e0-a575-eb11-9fe9-00155d53687a', 0, 0, N'56ce6b83-d992-eb11-951e-00155d53687a', CAST(N'2021-04-01T16:30:48.537' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (92, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'7eb447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'2f8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:51.957' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (93, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'7db447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'308bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:51.970' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (94, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'a6ed9380-9a75-eb11-9fe9-00155d53687a', 0, 0, N'318bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:51.970' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (95, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'874bd8cd-f35b-eb11-b39f-00155d53687a', 0, 0, N'328bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:51.970' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (96, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'2a34f4e0-a575-eb11-9fe9-00155d53687a', 0, 0, N'338bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.160' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (97, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'88cdfa9d-9a75-eb11-9fe9-00155d53687a', 0, 0, N'348bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.160' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (98, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'809ef6f0-a575-eb11-9fe9-00155d53687a', 0, 0, N'358bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.237' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (99, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'e6f2de48-a675-eb11-9fe9-00155d53687a', 0, 0, N'368bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.237' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (100, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', 0, 0, N'378bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.253' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (101, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'185e6c00-a675-eb11-9fe9-00155d53687a', 0, 0, N'388bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.253' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (102, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'10d28779-a575-eb11-9fe9-00155d53687a', 0, 0, N'398bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.440' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (103, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'56388d8b-a575-eb11-9fe9-00155d53687a', 0, 0, N'3a8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.440' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (104, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'46b37461-a575-eb11-9fe9-00155d53687a', 0, 0, N'3b8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.487' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (105, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'8ac98f50-a575-eb11-9fe9-00155d53687a', 0, 0, N'3c8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.503' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (106, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'06f8ef27-a575-eb11-9fe9-00155d53687a', 0, 0, N'3d8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.520' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (107, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'4c412298-a575-eb11-9fe9-00155d53687a', 0, 0, N'3e8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.520' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (108, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'd4cf3c77-a475-eb11-9fe9-00155d53687a', 0, 0, N'3f8bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.737' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (109, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'34dff6a7-a475-eb11-9fe9-00155d53687a', 0, 0, N'408bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.737' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (110, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'c0d9b760-a475-eb11-9fe9-00155d53687a', 0, 0, N'418bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.753' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (111, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'12487901-a575-eb11-9fe9-00155d53687a', 0, 0, N'428bdefd-dd92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:02:52.770' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (112, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'c852dcfe-a675-eb11-9fe9-00155d53687a', 0, 0, N'7df378a7-de92-eb11-951e-00155d53687a', CAST(N'2021-04-01T17:07:36.503' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (113, N'Calls Provider', N'Call Performance', 0, 1, N'79bfb760-0a93-eb11-951e-00155d53687a', CAST(N'2021-04-01T22:20:35.653' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (114, N'dc99302c-9276-eb11-9fe9-00155d53687a', N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', 0, 0, N'11ba8578-0a93-eb11-951e-00155d53687a', CAST(N'2021-04-01T22:21:15.590' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (115, N'222d673f-9276-eb11-9fe9-00155d53687a', N'7eb447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'5e49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:42.953' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (116, N'222d673f-9276-eb11-9fe9-00155d53687a', N'874bd8cd-f35b-eb11-b39f-00155d53687a', 0, 0, N'5f49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:42.970' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (117, N'222d673f-9276-eb11-9fe9-00155d53687a', N'7db447b4-f35b-eb11-b39f-00155d53687a', 0, 0, N'6049506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:42.983' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (118, N'222d673f-9276-eb11-9fe9-00155d53687a', N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', 0, 0, N'6149506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.093' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (119, N'222d673f-9276-eb11-9fe9-00155d53687a', N'4c412298-a575-eb11-9fe9-00155d53687a', 0, 0, N'6249506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.127' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (120, N'222d673f-9276-eb11-9fe9-00155d53687a', N'56388d8b-a575-eb11-9fe9-00155d53687a', 0, 0, N'6349506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.127' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (121, N'222d673f-9276-eb11-9fe9-00155d53687a', N'2a34f4e0-a575-eb11-9fe9-00155d53687a', 0, 0, N'6449506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.267' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (122, N'222d673f-9276-eb11-9fe9-00155d53687a', N'6bb7ae04-c25b-eb11-977d-00155d53687a', 0, 0, N'6549506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.267' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (123, N'222d673f-9276-eb11-9fe9-00155d53687a', N'10d28779-a575-eb11-9fe9-00155d53687a', 0, 0, N'6649506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.280' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (124, N'222d673f-9276-eb11-9fe9-00155d53687a', N'c852dcfe-a675-eb11-9fe9-00155d53687a', 0, 0, N'6749506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.377' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (125, N'222d673f-9276-eb11-9fe9-00155d53687a', N'185e6c00-a675-eb11-9fe9-00155d53687a', 0, 0, N'6849506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.390' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (126, N'222d673f-9276-eb11-9fe9-00155d53687a', N'2674900d-a675-eb11-9fe9-00155d53687a', 0, 0, N'6949506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.407' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (127, N'222d673f-9276-eb11-9fe9-00155d53687a', N'26695223-a775-eb11-9fe9-00155d53687a', 0, 0, N'6a49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.530' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (128, N'222d673f-9276-eb11-9fe9-00155d53687a', N'10c30810-a775-eb11-9fe9-00155d53687a', 0, 0, N'6b49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.580' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (129, N'222d673f-9276-eb11-9fe9-00155d53687a', N'aaa4857c-a675-eb11-9fe9-00155d53687a', 0, 0, N'6c49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.593' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (130, N'222d673f-9276-eb11-9fe9-00155d53687a', N'e6f2de48-a675-eb11-9fe9-00155d53687a', 0, 0, N'6d49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.640' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (131, N'222d673f-9276-eb11-9fe9-00155d53687a', N'eadf731d-a675-eb11-9fe9-00155d53687a', 0, 0, N'6e49506c-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:36:43.657' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (132, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'13125646-659c-eb11-951e-00155d53687a', 0, 0, N'8f83bd55-659c-eb11-951e-00155d53687a', CAST(N'2021-04-13T20:04:21.903' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (133, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'4977af00-709c-eb11-951e-00155d53687a', 0, 0, N'979a0b1a-709c-eb11-951e-00155d53687a', CAST(N'2021-04-13T21:21:26.217' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (134, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'2a34f4e0-a575-eb11-9fe9-00155d53687a', 0, 0, N'095981df-3bba-eb11-aaf2-00155d53687a', CAST(N'2021-05-21T19:23:17.987' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (135, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'0a5fa3bd-a575-eb11-9fe9-00155d53687a', 0, 0, N'0a5981df-3bba-eb11-aaf2-00155d53687a', CAST(N'2021-05-21T19:23:17.987' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (136, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'88516d64-5dcf-ec11-80ea-00155d23d79c', 0, 0, N'4b8a400b-5fcf-ec11-80ea-00155d23d79c', CAST(N'2022-05-09T11:42:46.923' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (137, N'Admin', N'Partners', 0, 1, N'99d5ab53-5fcf-ec11-80ea-00155d23d79c', CAST(N'2022-05-09T11:44:48.423' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (138, N'Admin', N'Partners', 0, 1, N'5433e1c6-61cf-ec11-80ea-00155d23d79c', CAST(N'2022-05-09T12:02:20.697' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (139, N'Admin', N'Partners', 0, 1, N'6e7d5c9b-7124-ed11-80ef-00155d23d79c', CAST(N'2022-08-25T17:59:48.477' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (140, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'88516d64-5dcf-ec11-80ea-00155d23d79c', 0, 0, N'1809a0f2-5c25-ed11-80ef-00155d23d79c', CAST(N'2022-08-26T22:04:26.600' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (141, N'cecf471a-9276-eb11-9fe9-00155d53687a', N'88516d64-5dcf-ec11-80ea-00155d23d79c', 0, 0, N'3272ab31-5827-ed11-80f0-00155d23d79c', CAST(N'2022-08-29T10:35:27.217' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (142, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'5ed0c22c-612b-ed11-80f1-00155d23d79c', 0, 0, N'ba7c3337-612b-ed11-80f1-00155d23d79c', CAST(N'2022-09-03T13:50:06.630' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (143, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'5ed0c22c-612b-ed11-80f1-00155d23d79c', 0, 0, N'ae8f9544-612b-ed11-80f1-00155d23d79c', CAST(N'2022-09-03T13:50:29.080' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (144, N'dbf26c28-82fe-439c-a82d-1d586fdaace5', N'88516d64-5dcf-ec11-80ea-00155d23d79c', 0, 0, N'2b5dbfa3-eab7-ed11-bc79-b0c090b35a86', CAST(N'2023-03-01T10:06:32.577' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (145, N'dbf26c28-82fe-439c-a82d-1d586fdaace5', N'88516d64-5dcf-ec11-80ea-00155d23d79c', 0, 1, N'4ffb40b8-ebb7-ed11-bc79-b0c090b35a86', CAST(N'2023-03-01T10:14:16.480' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (146, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'56388d8b-a575-eb11-9fe9-00155d53687a', 0, 0, N'aa31cdf1-cbb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T12:59:20.297' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[RoleMenus] ([Id], [RoleKey], [MenuKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (147, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'891d6bec-13c3-ed11-bc80-b0c090b35a86', 0, 0, N'adc703fd-13c3-ed11-bc80-b0c090b35a86', CAST(N'2023-03-15T15:00:14.507' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[RoleMenus] OFF
GO
SET IDENTITY_INSERT [dbo].[Roles] ON 
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'Admin', N'KR Administrator', 0, 0, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', CAST(N'2020-09-04T22:15:31.680' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Staff', N'KR Staff ', 0, 0, N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', CAST(N'2020-09-04T22:15:43.773' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (3, N'Mentor', N'KR Mentor', 0, 0, N'41554fe3-2553-4a68-ab93-3a99702a6182', CAST(N'2020-09-04T22:15:49.720' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (7, N'BDE', N'KR BDE', 0, 0, N'cecf471a-9276-eb11-9fe9-00155d53687a', CAST(N'2021-02-24T16:49:05.320' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (8, N'Calls Provider', N'KR Calls Provider', 0, 0, N'dc99302c-9276-eb11-9fe9-00155d53687a', CAST(N'2021-02-24T16:49:35.343' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (9, N'HR', N'KR HR', 0, 0, N'222d673f-9276-eb11-9fe9-00155d53687a', CAST(N'2021-02-24T16:50:07.600' AS DateTime), N'1', NULL, NULL)
GO
INSERT [dbo].[Roles] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (10, N'Partner', N'partner access only', 0, 0, N'dbf26c28-82fe-439c-a82d-1d586fdaace5', CAST(N'2023-03-01T09:57:48.390' AS DateTime), N'1', NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Roles] OFF
GO
SET IDENTITY_INSERT [dbo].[Segments] ON 
GO
INSERT [dbo].[Segments] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'Future', N'Future', 0, 0, N'f217c599-fea7-4017-b9ed-0eb93c23521c', CAST(N'2023-03-22T08:01:42.180' AS DateTime), N'vj', CAST(N'2023-03-22T08:01:42.180' AS DateTime), N'vj')
GO
INSERT [dbo].[Segments] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Option', N'Future', 0, 0, N'168b2be9-584e-42f0-8c0d-13dcb2db4879', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj')
GO
INSERT [dbo].[Segments] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (3, N'Cash', N'Future', 0, 0, N'c3bbe12f-02c9-4908-a0f2-5d5cdae9e530', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj')
GO
INSERT [dbo].[Segments] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (4, N'Commodity', N'Future', 0, 0, N'788dd4c9-ee05-4f4f-a33a-5fcd645e80ca', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj', CAST(N'2023-03-22T08:01:42.183' AS DateTime), N'vj')
GO
SET IDENTITY_INSERT [dbo].[Segments] OFF
GO
SET IDENTITY_INSERT [dbo].[ServiceCategories] ON 
GO
INSERT [dbo].[ServiceCategories] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'Education', N'', 0, 0, N'7081761f-cbb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T12:53:27.407' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[ServiceCategories] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Call Recommendations', N'', 0, 0, N'da8f63eb-0fb9-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T21:05:55.317' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[ServiceCategories] OFF
GO
SET IDENTITY_INSERT [dbo].[Services] ON 
GO
INSERT [dbo].[Services] ([Id], [Name], [Description], [ServiceCost], [ServiceTypeKey], [ServiceCategoryKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'10 Ka Dum ', N'', CAST(29999.00 AS Decimal(12, 2)), N'fbd0b245-26ff-4284-85e8-e276cd9ea03a', N'7081761f-cbb8-ed11-bc7a-b0c090b35a86', 0, 0, N'bdc2aa2d-cbb8-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T12:53:51.237' AS DateTime), NULL, NULL, NULL)
GO
INSERT [dbo].[Services] ([Id], [Name], [Description], [ServiceCost], [ServiceTypeKey], [ServiceCategoryKey], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Partner Algo Group', N'', CAST(0.00 AS Decimal(12, 2)), N'db6f1719-1e6f-4c39-ad9c-62f5ee6d2ed4', N'da8f63eb-0fb9-ed11-bc7a-b0c090b35a86', 0, 0, N'749b0cf6-0fb9-ed11-bc7a-b0c090b35a86', CAST(N'2023-03-02T21:06:13.200' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Services] OFF
GO
SET IDENTITY_INSERT [dbo].[ServiceTypes] ON 
GO
INSERT [dbo].[ServiceTypes] ([Id], [Name], [Description], [IsRecurring], [RecurringTime], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'education', N'', 1, 30, 0, 0, N'fbd0b245-26ff-4284-85e8-e276cd9ea03a', CAST(N'2023-03-02T12:40:21.993' AS DateTime), N'1', CAST(N'2023-03-02T12:40:21.993' AS DateTime), N'1')
GO
INSERT [dbo].[ServiceTypes] ([Id], [Name], [Description], [IsRecurring], [RecurringTime], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (2, N'Call Recommendation', N'', 1, 30, 0, 0, N'db6f1719-1e6f-4c39-ad9c-62f5ee6d2ed4', CAST(N'2023-03-02T21:05:38.850' AS DateTime), N'1', CAST(N'2023-03-02T21:05:38.850' AS DateTime), N'1')
GO
SET IDENTITY_INSERT [dbo].[ServiceTypes] OFF
GO
SET IDENTITY_INSERT [dbo].[Status] ON 
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (1, N'Active', N'', N'Currently in use or available', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (2, N'Inactive', N'', N'Not currently in use or unavailable', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (3, N'Pending', N'PO', N'Waiting for further action or approval', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (4, N'Completed', N'', N'Finished or accomplished', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (5, N'Failed', N'', N'Unsuccessful or not completed as intended', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (6, N'Cancelled', N'', N'Terminated or no longer in effect', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (7, N'On Hold', N'', N'Temporarily paused or delayed', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (8, N'Archived', N'', N'No longer relevant but retained for reference', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (9, N'Draft', N'', N'Not yet finalized or ready for use', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (10, N'Approved', N'PO', N'Authorized or given permission', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (11, N'Rejected', N'PO', N'Denied or not approved', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (12, N'Payment Received', N'PO', N'Payment Received approved', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (13, N'Pending Approval', N'PO', N'Awaiting authorization or permission', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (14, N'Scheduled', N'', N'Planned for a future date and time', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (15, N'Overdue', N'', N'Past its due date or deadline', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (16, N'Urgent', N'', N'Requires immediate attention or action', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (17, N'Normal', N'', N'No special urgency or priority', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (18, N'High', N'', N'Requires elevated urgency or priority', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (19, N'Low', N'', N'Lower urgency or priority than other tasks', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (20, N'Open', N'', N'Available for use or acceptance', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (21, N'Closed', N'', N'No longer available or accepted', NULL)
GO
INSERT [dbo].[Status] ([Id], [Name], [Category], [Description], [Code]) VALUES (22, N'Customer', N'Lead', N'LTC', NULL)
GO
SET IDENTITY_INSERT [dbo].[Status] OFF
GO
SET IDENTITY_INSERT [dbo].[Strategies] ON 
GO
INSERT [dbo].[Strategies] ([Id], [Name], [Description], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy]) VALUES (1, N'HNI Service', N'', 0, 0, N'4bca2869-b4c7-ed11-bc88-b0c090b35a86', CAST(N'2023-03-21T12:18:40.047' AS DateTime), NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Strategies] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (1, N'Adhya', N'Ram', N'9620037137', 0xF9365B5EA446083D3DA913A8803EC7214AEED1F2E8FDBA108E27D9B6FB135437DEC7AC07A7615D1DAAC9597A74099C575B1C07C1CC95C169F4AE17A9B57BD390, 0xC601660953484D2F1C17E142B3967F1C9F70F136418799B7C060EF8A41F03372FF728EB61B210433D41E9115541A293202DF917EABE706C4B2339FD55E3C05372C710EBF85B8E8A6F0D4874444C0BC79BE27DB6A491A63EAF9ED6A4E829BCFDCB855185B4AD49021828BFA2B839D0C0E8D3F41D5DF9708066A01E8C56F0FAA49, N'adhyaram@jalattics.com', NULL, NULL, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'female', N'', NULL, 0, N'84d5f2fe-3157-43b4-a43f-d1298a87a0c9', CAST(N'2020-10-13T14:59:01.927' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (2, N'Rakshith', N'DKG', N'9620037137', 0x4CAEEB0526917A419B02511AEE60562B5B0828890D356889BB1DFBFB015723B1B7E13E72BDC230B9F60F2EDF1524494CA046CC3C0DAE36F55A92604BCE93CC3A, 0xFFFB6C3E92EF00FA066090AE9FDD08A3984CFD90FF5B8136D6227FA354ADFD0889926EC5E63A9284C4DA638860E5CF84ED8981C5F3C1BA8F79F03E1D46C846F8AE577F17F113B30FE6C0F87EF408395E560DAF0C1D3E06AB07EDB5380F188FF70CDA3521E10F6DFF4FD72CDCF4159C4C7468DBAE3E499A6327DAFA724BDFD912, N'rakshith@jalattics.com', NULL, NULL, N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'male', N'', 0, 0, N'dc9a15e4-3a0d-eb11-bfa7-00155d53687a', CAST(N'2020-10-13T15:30:16.077' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (3, N'Dayanand', N'G', N'9740249079', 0x4AF226F40AE23C4007C3176A7E892EF81EDC773BE644887DA96AFD067B73F7F39471BDFEAFA840A4DEF96A64C44E50C8CA0FFBF2E8B0CF2EFC46AC0BD7B8B9E1, 0x666C6666D999AEAD1706E3DD97EC8F20E08B3EF43538683E6B31F2A592BBBBA41EBA48C4734F9C06189FB0F00495A28E3E974258145EE9B818C55F68DB4E305450233DEDD00CAF1D50826A8CD706442F2C0429CDA1B4B7014D0783CF9770B29DEF019EDFC892FB0246F54C5541225B59CC6F41432A8A2F6840196AD9251D47DA, N'dayanand@jalattics.com', N'', N'No 20 A 2nd cross', N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'male', N'', 0, 0, N'36a4ad69-5f0d-eb11-bfa7-00155d53687a', CAST(N'2020-10-13T19:51:42.100' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (4, N'Satish', N'Kumar', N'9620037137', 0xA78B143324EE13AF97E961A75112B8D8813E839DB65BB764FB9A2F30CD315CECF8E496DA6454E2AA88964BFD7FEB2DE77ACDABFAC766CFBFDBD885C774095360, 0xF1D386E7DDF8A39FE9613019267466D1858E5A8556C754161ACA6526B70BB3209E0477485A7A550E0806C68C11B0D10EFFEF41F14CDC6FDD246DF7778FEE3B75CD73BCF0E562BCC2A8C29C0D62109D2877535DC46384E95EED72188CDE0DD65D9E7AE43E9659DBFC6A3F7653A9621F43605243E966909CE7844EF3E9FED27F17, N'satish@jalattics.com', N'', N'No 20 A 2nd cross', N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'male', N'', 0, 1, N'76127214-600d-eb11-bfa7-00155d53687a', CAST(N'2020-10-13T19:56:28.603' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (5, N'Sandeep', N'S', N'9620037137', 0x8F882695549F6F35302E4834587069E2594F9A846C85BF4E4E9337A20F12CF1CE8623F20F004371B19E82E0CA6B246BD57C7BBC5D2DA718B9E0CA9D841B6D600, 0xA35386F1BF7676B7E0DFC31CAC1738B58181BD4C2481421D713067DC0A5E420E66A13504FC2DB909418C3FB30C89B7357247FACD18478016987418F6B68F55F678C2473F45375EB68677FF91A6F5ABE515837FF130949D42E973C00715B752B7DDA7CAB962EC80F0F1C215E61A62A23929F755869B8C33445D9177AB19817316, N'sandeep@jalattics.com', N'', N'No 20 A 2nd cross', N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'male', N'', 0, 1, N'80981f3b-600d-eb11-bfa7-00155d53687a', CAST(N'2020-10-13T19:57:33.503' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (6, N'Vijay', N'Sahu', N'9411122233', 0x4AAB2459BB001028A2558D28CAEB94F99CFD6735819F38BA75F919C70E94F8AAE8E448BA6937EF8EAE985362CD5E10AA8C3D86BACA9A0383D01FC045D2A9D857, 0x50BCE34ACBE3BFEB37BBCACF557E0D9E92D5BB8C2ED3372AC80EF06B44A32FA7713AB35C80524939CF7BFCA3237B04E08198AFD07B879DFCB9F7FEEDF3027A1FEEA33DE04CDA0CF4F4E8736BBFE2B35975DDC222AE3EAA44DA73334982BF715E3E1D68938145DFE583B19DCDA55320EB4D735A3597C9D62EDD1E0E7226E52830, N'vijay.sahu12@gmail.com', N'', N'Mansha , Wagholi', N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'male', N'', 0, 0, N'bb74d26f-aa28-eb11-bee5-00155d53687a', CAST(N'2020-11-17T13:26:46.037' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (7, N'sailesh', N'Behera', N'9703198655', 0x3771AE370B1B3511735F7E701EEBAD5B3E235B449CB9A786EA92291C05DEA30DAA34E9804153F3BD7BDAD85305DA51FD35247B8B66D031373963366961CEA14F, 0x2C482CE55A4B7BAE8AC2EA11E20D20387B39BF942E0F89C865FD8A522BB79C127F883270D4666FE244668B10BF0C951F8ED79D764971D376571A34E9758DF4C004D79FAAB423F04DBB3C75161C7BBBA954E2B2FD0317AD0CC03C8270A36394E81956C1FB6DF860D47FF200D28DD860C511C597CCC24758BF6CD024300577AD67, N'bsaileswar@yahoo.com', N'03-09-2020', N'Teja Apt, BabaNagar, Nacharam, Hyderabad', N'84f4fa8b-db6a-4d1e-917d-5862b376ccd3', N'male', N'', 0, 1, N'4d4c968c-ed65-eb11-9e64-00155d53687a', CAST(N'2021-02-03T12:30:51.520' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (8, N'Banasmita', N'Sahoo', N'7327872756', 0xDC724FE6012FD17DB4BFFB3956A304FB96CF558C26C670C42DA6E0A8CF80627E79B76D86AB3F249FBA3D7E0C4F72B4AFD2EF01444F9A8285DCD4F0FB4762F7B4, 0xBAB8F01057EAE7E8A9B2E0D97A5F279E82731129A1048E358CD1A77D86463D4B795EBBE85017E9427C386E7BCDA577F5DA074A76D2F52B02EE7F2DE2818372FC930A776E9F44A715DB77B9EE1B642C0E54A1ADAACD88285E09F327BF138F13E4F35E406AAECB6F978F43FB14C60C4978E3A506B8A0E64CC28012524CFA870996, N'king157.sahu@gmail.com', N'02-March-2021', N'Hitech City ', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'ae4a96bd-1286-eb11-94db-00155d53687a', CAST(N'2021-03-16T10:17:42.323' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (9, N'Ritu', N'', N'9098648073', 0xA8D2CC9727F948A764CAC50E7E3C3EF49E31189D52CA2E791FE3552BFCF81CD2BEE65F99295D794DFDBC2FE87292F2C93D5717F391823CEF910A45ECF8E7903D, 0xB0252323F07E3DA4D3808EA244F285AFA464DBCCC1072C75C59E016C0F5A5F58B398FBF8AA91DBB4F4529542BA3B970274D899E751EB1780D6223CDC67573DB99C8AF3A8235AF85EEF02F389E4ED740C13AE1FD1E0B5E0711C6112D64DD56E9D0CE80844FFA99BBC91630762F4C93E85B1D6380320B2CE64C224726115A821BE, N'king161.sahu@gmail.com', N'02-March-2020', N'Indoor', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'1ee1095e-a687-eb11-94db-00155d53687a', CAST(N'2021-03-18T10:26:58.700' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (10, N'Nishant', N'Duphare', N'8793885079', 0xD1354B72537A56D1F91956DF9925E3EA02AE6002B29901E697EC9E317129E57981240AA180264C25E89288163B75A6DCDA32445F5CD018B59968758D1EC8FCDC, 0x0B2A847DED498549E674C39A4ED21B3E7AFC1134104541AFA32D69903E5C5C7CA4BEBEDA9C4C58EC395AF1D6821FF94B74615CCF97E1D00E499867F2A1F9560198EC2B56A027779005BAB007970BCC4F5505C76B497FB357E01D25C78EF955859109345C1469086DBF375FAF2B5F9C1368AD42203F1DA635F2592CB4770A27C8, N'king152.sahu@gmail.com', N'02-March-2020', N'Nagpur', N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'male', N'', 0, 0, N'd801c580-a787-eb11-94db-00155d53687a', CAST(N'2021-03-18T10:35:06.450' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (11, N'yashi', N'jadon', N'9310732580', 0xFADE775A5D5D286F50A8CE165148C9247E0F82EDE186D589C75E1FB4FDF0F3254E9E962148150C71343F5B881AC3478811F7146576B09EE60B51D1377405E5D1, 0x3543ED8D44946605AEBE4A54A2C7E66802DDBAF305E36E267F45DE6A147953FC03B96322F02FCD6A12B052D6EBAE4872455DDA20E702AE1341EE6E1F98C50AF820C24AEE2A413EE849B9118171AAFEAA94B2D5D5C861230B62E550A13A08890DE75DAB376DE6924C78317CE5D3010F065B3CDF8736340951832EB14FA11C486D, N'king162.sahu@gmail.com', N'02-March-2020', N'Indoor', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'e616912b-a987-eb11-94db-00155d53687a', CAST(N'2021-03-18T10:47:02.480' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (12, N'Abhishek', N'Shukla', N'9779035041', 0x4DCA06D94A1388EECAEB7E8CD690CFD21DE19538DA121F1772EEE8E9E77920A5A27E82E3FE84AB9D1325F30E6C1A0971ACC8D52102A3345636E5FF637B0F3400, 0x72E7BCB8DFE63822CE6A589C32380028CDE7D472AA698BA6C293CAF2D4E9589CD3CCAA4142DE0477E97DD10524318E67A0E731AD1B702706B85CB051DEF2AEFD4D7350C8777A8169804C3F95E732DAF75C7B3F49099252E3A55102D4ED405C04D4F10609CB09F1836319B307EC3E29DFFA89E936ED6C331DF4303E8187BD1537, N'king155.sahu@gmail.com', N'2020-01-01', N'Delhi', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'male', N'', 0, 0, N'e03f1d44-d38b-eb11-94db-00155d53687a', CAST(N'2021-03-23T17:58:27.197' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (13, N'Deepali', N'Kholi', N'8448578899', 0x8308A001B3E511040610405CF899395264F7C27CE235874FB92AD5BEFC0EBD68C8AAC1C2ECB18E918A9398E075EBE83DCC1500146D5358705A55B7B237A920CA, 0x28F066A683798AF1338C091058FA2E9D811C03B772FF75C1B9332A17418203C51BAF54C4FC31611B2D85D92D3C58377397B7D601C4629F3C56D94A6B271AFF02CB57DE7E22EA26E5A45A5DD56E0FB4834A27BFD1910B5C590CE2C92D79670B54912C4823C3ED1BB312B4B50C9E9544F297A680EDE64D9E7620D56BFE40F87F86, N'king156.sahu@gmail.com', N'2020-01-01', N'Delhi', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'1710c983-8b8c-eb11-94db-00155d53687a', CAST(N'2021-03-24T15:57:21.433' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (14, N'Maithili', N'Muley', N'7038659389', 0x0A10D0AEFD78C41C0E18A22A75499D7330A5B09C3D015DBC596022441CE308A356E70781FAC4E12833AF2DF1EB071119ACBD0052759D96E142347CB154A2A4CB, 0x79F602FA930BFC8FF7C9D1DA85EDFDEF2926F89ED1A76E0BB47195C579DB7CDC31682EEBC5E7C882764DABCDAC692ED66E83347AE0CFC0FE83BF59F19A6F8BBD19A047229B3CF46F0FE803CC6EC2C14D4B90E664DDD036F367BE29167577DA06B94477CD446305D5855F7590832B0E05B2F7B5EF7CCE289412C3937489C43467, N'king173.sahu@gmail.com', N'01-april-2021', N' balaji nagar, shri ram colony, KPR Colony, Kuppal Gudda , Telangana 500089', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'57efa123-a492-eb11-951e-00155d53687a', CAST(N'2021-04-01T10:08:44.500' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (15, N'Rajnikant', N'', N'8142824343', 0x766FA668A3D266786B6D87B552C27809980275D9B6959F18267DB68ABF7B0382ECD8C9A5B0DC2D01CEC99037099D9531DDE4868F881AE46EDFB34AD8966C21A1, 0xE9FA607FD8B5BCF73991CC2C2F0FC19583E09529F998503869DB928FAF9DFA0218821B346074343CA370D04E804B56A740779E12785D92D01F800E238C15AECA4A09613B880F7DFB515C861BA7AAE338E439FC1D410B57C3154D9166291BDF01987EDB50C24B3FF6C2281CD7BE080C057082421912B327BE687AA4F5F7A70FBB, N'king159.sahu@gmail.com', N'01-01-2020', N'', N'dc99302c-9276-eb11-9fe9-00155d53687a', N'male', N'', 0, 0, N'8278cf5c-da92-eb11-951e-00155d53687a', CAST(N'2021-04-01T16:36:53.253' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (16, N'Darshit', N'Adhyaru', N'9558809013', 0x0280F9BCF9040C9D79F241880E73D8736D97FD0E57A93789B097B397B6E864AD2B03FBBE149E9E813C91D3B9023DE15EE8CC75AE492D43374BE70183B09DBCE3, 0xEDA7680A6718F0A101B2631AB5922FE815BA4C83481CC86C8E69D9497D4390D6CF719F71227F2FA4BEB0B8C12A037E400D5B604AD259197969AE0A5E61B9A9A5CB32B16DB5AC54FA6696A0D0567CDECE60D85D25074DBDD4D3D40A71A2288D2FEA1B1CBC8330BD081EAF5BE475BDD3012BD29D7F69D6FCD968748C1849497E20, N'king160.sahu@gmail.com', N'01-01-2021', N'Shri Sai Krupa  Block No. 46 / A Ghanshyam Residency  Railnagar,  Popatpara Rajkot - 360001', N'dc99302c-9276-eb11-9fe9-00155d53687a', N'male', N'', 0, 0, N'50ff1a9e-da92-eb11-951e-00155d53687a', CAST(N'2021-04-01T16:38:42.800' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (17, N'Bijeta  ', N'mohapatra', N'7787978858', 0x566D40E3D03B4ABBCD926EB2FE57A1752CBE969C2DFBEC0D29384B0F2DFE76A1BF098D1BA63AB6BD2BDFD009534B4F37BB79491ECD9A3C0C1B156126CB3B3D24, 0x323CBB4E39D05BD4D3C44B26603C5703E9C33123BCFCA86BFCE6EA56BD9A2229E8C2AA3F5E7A0E2FB73A08184F0D88E86FC0F3199F5B10010D9278C2B155403CA7E1ABAE256D1FA06DFCBEAD4FB0029F544907BBD3897B8AAC88CF8D9F22FA7B946318BE8BE900063E3AB19B797D04DB1334ACFDB78A260BD088F53F33343D3E, N'king174.sahu@gmail.com', N'07-April-2021', N'Nandi Nagar, road no. 15 , banjara hills, 500034 , Telangana ', N'222d673f-9276-eb11-9fe9-00155d53687a', N'female', N'', 0, 0, N'3ee08e79-5e97-eb11-951e-00155d53687a', CAST(N'2021-04-07T10:32:39.687' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (18, N'mastan', N'babu', N'9666909047', 0x244F87D3C631151094912D265F563D947FED9FC0CB59060D8DA265DAD7896C0689D6BE9E79142505FFC89555474AA4A4E86E03E18E194C564D18D9008BA682D6, 0x6003CC622062014436DA06A96EC3078487D652CB2AB8CD24AA3499987335A85586D0AFE232229390BB2EC182F81D6E61A959739DC33F31DFDED5F26B42A1A7FD117C7BBCCB69A12A875C16AB4C71C0014559103E0E983C5A033F748DDC492496A4CF1528667F7CD7DBCF7A79E16F5538149344907D2AE17E62AEF34CEAD9B0EB, N'king175.sahu@gmail.com', N'20th March 2021', N'8-3-250/B/1/A Yusuguda, Hyderabad , 500045 , Telangana ', N'cecf471a-9276-eb11-9fe9-00155d53687a', N'male', N'', 0, 0, N'18a3ff0d-6797-eb11-951e-00155d53687a', CAST(N'2021-04-07T11:34:04.720' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (19, N'Plan', N'User', N'9638527410', 0x54A98182968F8AA7F26D1EC6A9E3583B6AB53BB301976440D36E8A810A630D7533F2BC15F5A865EF58A221BAF0AC4E6E102973D5D86CFBE95E8C7D4B6B79DB54, 0xB7B8F70C3B6CF90D66250039FB79D03466151CC2FD845636ED4CDA1759B84C1EB89D76FDF2060514D00749D4103FD3138D8352EF957034D82ED451CABE1FF089301966F07DE23F7169E6D42237EB43B6C68EBBC2C2E5FFF388BA432EC829279A738F3B1CD49C6DC41590292F8F1B4E0EF7711F44D13B1A0C063A3F2862DD95DB, N'media@jalattics.com', N'', N'R T Nagar', N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'male', N'', 0, 0, N'53aa230f-ac9b-eb11-951e-00155d53687a', CAST(N'2021-04-12T21:58:06.560' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (20, N'Test', N'Admin', N'9834724856', 0x0C0E64A3807682E4C47D84EAC5CEC527CC56470AF7EB132E492A283E2DB1713A6DB693E5431BD1D0229DB4C401E0D4836B24BB76B1FD29DCF86D108615D15ECD, 0x19390BB846E74B8888A5DE7C93AA33A82E86B00D9E601BD174B6C82EE207129DC47ACA16A07DC3BFE7A2622A7BAC88EFBFB4F8C8084F73607F37B00374F09CEA81BBED18C9D24A2B65E08FE473EA0BCF676917E7F910D6FB52171874F8E38E9FBCAC6625CA03333CF63474D8BE0901092D68B29E3E31AB5D45C713C2EB6742FB, N'chikate.ninad00@gmail.com', N'2020-05-19', N'Nagpur', N'd4ce182f-8ffb-4ec4-8dc5-d3b760f9231b', N'male', N'', 0, 0, N'3ca214d0-8cb8-eb11-aaf2-00155d53687a', CAST(N'2021-05-19T15:57:30.100' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (21, N'Pankaj', N'Bharadwaj', N'8010808014', 0x2580EB044E8C6696535C32AC6C0A7BA7DCF8D78A1CE3EB37FB4A4E57B5756F9F6554FE77A027B7174056CD103BDAC05A05C52C9EE74F2748E443C2E810C0EC7B, 0x25B2C07AACAAF28B593E5BE0D9903E0061C6E95EDB9637D039391D86F38E7E4F04071D7B4F81C78C5F4F8EDF610A27173801E7A8C67AF9B55B9918151B6F057B979EF92D3DEB109C1B02868E8D513889E6CE789119C4DCFF2AFBE971563A4B8F45E2F61D25DF5F7FA020EB1FE0F6FE2796A2B52C1DBAE1AAC10CE5D1672517E2, N'pakaj.bharadwaj@gmail.com', N'2019-01-01', N'New Delhi', N'dc99302c-9276-eb11-9fe9-00155d53687a', N'male', N'', 0, 0, N'1a7104d1-38ba-eb11-aaf2-00155d53687a', CAST(N'2021-05-21T19:01:16.253' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
INSERT [dbo].[Users] ([Id], [FirstName], [LastName], [MobileNumber], [PasswordHash], [PasswordSalt], [EmailId], [DOJ], [Address], [RoleKey], [Gender], [UserImage], [IsDisabled], [IsDelete], [PublicKey], [CreatedOn], [CreatedBy], [ModifiedOn], [ModifiedBy], [Password], [PasswordKey]) VALUES (22, N'Kuldeep', N'', N'8178221079', 0xD59E543815B3146FA9505472275FA5DD3835D1ABC9B438B631B61A13780FF8B6696CA19D66FB00B845BC62F5F826C755FED3774FF33AAE964A5AE602228AB2DB, 0x6C9A6F7B26B152555E900106E1D540565B63438812C9CCECB1AF09977827CB9DBE818C10823058995CB3FFF1EAAE744EA348FE15460C1FA23F160DCB70E27672B780A75618FEA5D43581793043E98474AEF1AC4671B11E34DE357F550C0153E8FB19A1C9E9984754CF7DE7BC4FE0EE330264A9B1AC153C072774FAC66C564C3C, N'king213.sahu@gmail.com', N'', N'New Delhi ', N'dbf26c28-82fe-439c-a82d-1d586fdaace5', N'male', N'', 0, 0, N'7ce6e32e-5c25-ed11-80ef-00155d23d79c', CAST(N'2022-08-26T21:58:58.230' AS DateTime), NULL, NULL, NULL, NULL, NULL)
GO
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[UserTypes] ON 
GO
INSERT [dbo].[UserTypes] ([Id], [Name]) VALUES (1, N'Lead')
GO
INSERT [dbo].[UserTypes] ([Id], [Name]) VALUES (2, N'Customer')
GO
SET IDENTITY_INSERT [dbo].[UserTypes] OFF
GO


GO
insert into EmailTemplates values ('SubscriptionExpired','Subscription About To expired', 'Hello {{receiver}} , Your Subscription {{subscriptionName}} going to expire in {{days}}, Please renew to avoid the important updates.',GETDATE(), GETDATE(),'BB74D26F-AA28-EB11-BEE5-00155D53687A')
GO

GO
SET IDENTITY_INSERT [dbo].[Analysts] ON 
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (1, N'1ee1095e-a687-eb11-94db-00155d53687a', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (2, N'8278cf5c-da92-eb11-951e-00155d53687a', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (3, N'50ff1a9e-da92-eb11-951e-00155d53687a', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (4, N'1a7104d1-38ba-eb11-aaf2-00155d53687a', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (5, N'8dd53404-f2cd-ed11-8110-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (6, N'336da113-4cd5-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (7, N'a09e071d-4ed5-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (8, N'e3eedf57-2bd6-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (9, N'cc083981-2bd6-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (10, N'2eb5f59e-2bd6-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (11, N'91ee36e5-2bd6-ed11-8111-00155d23d79c', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (12, N'5cac2494-1503-4f0f-aac7-f9ea6fff8ebb', N'analyst', 1, 1)
GO
INSERT [dbo].[Analysts] ([Id], [UserKey], [UserType], [IsActive], [IsDelete]) VALUES (13, N'1fcbc042-b573-443b-8498-69ccfafeb84e', N'analyst', 1, 1)
GO
SET IDENTITY_INSERT [dbo].[Analysts] OFF
GO


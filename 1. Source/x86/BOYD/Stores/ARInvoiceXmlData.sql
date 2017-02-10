CREATE VIEW [dbo].[ARInvoiceXmlData]
AS
SELECT     T0.DocEntry, ISNULL(T0.DocNum, N'') AS invoiceNumber, T0.DocDate AS invDate, ISNULL(T0.U_AE_PDateFr, N'') AS invCoveredFrom, ISNULL(T0.U_AE_PDateTo, N'') 
                      AS invCoveredTo, ISNULL(T0.U_AE_FinalInv, N'') AS invFinalInvoice, GETDATE() AS invArchiveDate, T0.CardName AS invtoName, T0.CardCode AS invtoID, 
                      ISNULL(T0.U_AE_Member, N'') AS invtoTo, ISNULL(T1.Name, N'') AS invtoContactName, ISNULL(T1.E_MailL, N'') AS invtoEmail, ISNULL(T0.U_AE_YourRef, N'') 
                      AS invtoClubReference, ISNULL(T2.CardFName, N'') AS invtoAlias, ISNULL(T2.LicTradNum, N'') AS invtoVATNumber, ISNULL(T3.Street, N'') AS invtoLine1, 
                      ISNULL(T3.StreetNo, N'') AS invtoLine2, ISNULL(T3.Block, N'') AS invtoLine3, ISNULL(T3.City, N'') + ' ' + ISNULL(T3.ZipCode, N'') + ' ' + ISNULL(T3.Country, N'') 
                      AS invtoLine4, ISNULL(T0.U_AE_Member, N'') AS memberName, '' AS memVATNumber, ISNULL(T0.U_AE_MAdd1, N'') AS memLine1, ISNULL(T0.U_AE_MAdd2, N'') 
                      AS memLine2, ISNULL(T0.U_AE_MAdd3, N'') AS memLine3, ISNULL(T0.U_AE_MAdd4, N'') AS memLine4, ISNULL(T4.CompnyName, N'') AS invfromName, 
                      ISNULL(T4.Manager, N'') AS invfromContactName, ISNULL(T4.E_Mail, N'') AS invfromEmail, ISNULL(T4.Phone1, N'') AS invfromTelephone, ISNULL(T4.Fax, N'') 
                      AS invfromFaxNo, ISNULL(T0.U_AE_YourRef, N'') AS invfromClubReference, ISNULL(T5.Street, N'') AS invfromLine1, ISNULL(T5.StreetNo, N'') AS invfromLine2, 
                      ISNULL(T5.Block, N'') AS invfromLine3, ISNULL(T5.City, N'') + ' ' + ISNULL(T5.ZipCode, N'') + ' ' + ISNULL(T5.Country, N'') AS invfromLine4, 
                      ISNULL(T0.U_AE_VesselName, N'') AS invinfoName, ISNULL(T0.U_AE_VoyageNo, N'') AS invinfoVoyageNumber, ISNULL(T0.U_AE_DIncident, N'') 
                      AS invinfoIncidentDate, ISNULL(T0.U_AE_PIncident, N'') AS invinfoDischargePort, ISNULL(T0.U_AE_Case, N'') AS invinfoDescription, ISNULL(T0.DocCur, N'') 
                      AS invinfoCurrency, ISNULL(T4.CompnyName, N'') AS payeeName, ISNULL(T4.RevOffice, N'') AS payeeRegistration, ISNULL(T4.TaxIdNum, N'') AS payeeVATNumber, 
                      ISNULL(T5.Street, N'') AS payeeLine1, ISNULL(T5.StreetNo, N'') AS payeeLine2, ISNULL(T5.Block, N'') AS payeeLine3, ISNULL(T5.City, N'') + ' ' + ISNULL(T5.ZipCode, 
                      N'') + ' ' + ISNULL(T5.Country, N'') AS payeeLine4, ISNULL(T7.BankName, N'') AS bankName, ISNULL(T8.Street, N'') AS bankLine1, ISNULL(T8.StreetNo, N'') 
                      AS bankLine2, ISNULL(T8.Block, N'') AS bankLine3, ISNULL(T8.City, N'') AS bankLine4, ISNULL(T4.CompnyName, N'') AS AccountName, ISNULL(T8.Account, N'') 
                      AS Number, '-' AS SortCode, ISNULL(T8.SwiftNum, N'') AS SwiftCode, ISNULL(T6.LineTotal, 0) AS invsumTotalFee, ISNULL(T6.LineTotal, 0) 
                      AS invsumTotalDisbursements, ISNULL(T6.VatSum, 0) AS invsumTotalTaxes, ISNULL(T6.LineTotal, 0) AS invsumTotalThirdPartyFee, 
                      0.0 AS invsumTotalThirdPartyDisbursements, ISNULL(T6.VatSum, 0) AS invsumTotalThirdPartyTaxes, ISNULL(T0.DocTotal, 0) AS invsumAmountPayable, 
                      T0.DocDate AS paymentsFeeDate, ISNULL(T6.Dscription, N'') AS paymentsContractor, ISNULL(T9.InvntryUom, N'') AS paymentsUnit, ISNULL(T6.Price, 0) 
                      AS paymentsCost, ISNULL(T6.Quantity, 0) AS paymentsQuantity, ISNULL(T6.LineTotal, 0) AS paymentsAmount, ISNULL(T6.Text, N'') AS paymentsWork, 
                      ISNULL(T6.Dscription, N'') AS paymentsDisbursementType, ISNULL(T6.Text, N'') AS paymentsDisbursementComments, ISNULL(T6.LineTotal, 0) 
                      AS paymentsDisbursementAmount, ISNULL(T6.VatSum, 0) AS paymentTotalLocal, ISNULL(T6.VatSum, 0) AS paymentTotalVAT, ISNULL(T6.VatSum, 0) AS paymentTotal, 
                      T0.DocDate AS thirdpartypaymentsFeeDate, ISNULL(T6.Dscription, N'') AS thirdpartypaymentsContractor, ISNULL(T9.InvntryUom, N'') AS thirdpartypaymentsUnit, 
                      ISNULL(T6.Price, 0) AS thirdpartypaymentsCost, ISNULL(T6.Quantity, 0) AS thirdpartypaymentsQuantity, ISNULL(T6.LineTotal, 0) AS thirdpartypaymentsAmount, 
                      ISNULL(T6.Text, N'') AS thirdpartypaymentsWork, ISNULL(T6.VatSum, 0) AS thirdpartypaymentTotalLocal, ISNULL(T6.VatSum, 0) AS thirdpartypaymentTotalVAT, 
                      ISNULL(T6.VatSum, 0) AS thirdpartypaymentTotal
FROM         dbo.OINV AS T0 LEFT OUTER JOIN
                      dbo.OCPR AS T1 ON T0.CntctCode = T1.CntctCode INNER JOIN
                      dbo.OCRD AS T2 ON T2.CardCode = T0.CardCode LEFT OUTER JOIN
                      dbo.CRD1 AS T3 ON T3.CardCode = T0.CardCode AND T3.Address = T0.PayToCode INNER JOIN
                      dbo.OADM AS T4 ON 1 = 1 INNER JOIN
                      dbo.ADM1 AS T5 ON 1 = 1 LEFT OUTER JOIN
                      dbo.INV1 AS T6 ON T6.DocEntry = T0.DocEntry INNER JOIN
                          (SELECT     TOP (1) BankCode, BankName
                            FROM          dbo.ODSC) AS T7 ON 1 = 1 LEFT OUTER JOIN
                      dbo.DSC1 AS T8 ON T8.BankCode = T7.BankCode AND T8.County = T0.DocCur LEFT OUTER JOIN
                      dbo.OITM AS T9 ON T9.ItemCode = T6.ItemCode

GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[31] 4[4] 2[47] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "T0"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 125
               Right = 234
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T1"
            Begin Extent = 
               Top = 6
               Left = 272
               Bottom = 125
               Right = 432
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T2"
            Begin Extent = 
               Top = 6
               Left = 470
               Bottom = 125
               Right = 631
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T3"
            Begin Extent = 
               Top = 6
               Left = 669
               Bottom = 125
               Right = 829
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T4"
            Begin Extent = 
               Top = 6
               Left = 867
               Bottom = 125
               Right = 1027
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T5"
            Begin Extent = 
               Top = 126
               Left = 38
               Bottom = 245
               Right = 198
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T6"
            Begin Extent = 
               Top = 131
               Left = 239
               Bottom = 250
               Right = 401
            End
            DisplayFlags = 280
            TopColumn = 0
         End' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ARInvoiceXmlData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'
         Begin Table = "T8"
            Begin Extent = 
               Top = 126
               Left = 634
               Bottom = 245
               Right = 794
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T9"
            Begin Extent = 
               Top = 126
               Left = 832
               Bottom = 245
               Right = 1011
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "T7"
            Begin Extent = 
               Top = 6
               Left = 1065
               Bottom = 95
               Right = 1225
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ARInvoiceXmlData'
GO

EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ARInvoiceXmlData'




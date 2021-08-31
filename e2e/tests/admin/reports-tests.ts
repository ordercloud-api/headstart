// import { t } from 'testcafe'
// import testConfig from '../../testConfig'
// import { adminTestSetup, adminClientSetup } from '../../helpers/test-setup'
// import adminHeaderPage from '../../pages/admin/admin-header-page'
// import { delay } from '../../helpers/wait-helper'
// import reportsTemplatesPage from '../../pages/admin/reports-templates-page'
// import reportsProcessPage from '../../pages/admin/reports-process-page'
// import mainResourcePage from '../../pages/admin/main-resource-page'
// import { BUYER_LOCATION_HEADERS, createReportTemplate, deleteCreatedTemplate, LINE_ITEM_HEADERS, PURCHASE_ORDER_DETAIL_HEADERS, SALES_ORDER_DETAIL_HEADERS } from '../../api-utils.ts/reports-util'
// import { ReportTemplate } from '@ordercloud/headstart-sdk'

// // report types for eventual scaling of coverage:
// const buyerLocationOption = 'Buyer Location Report'
// const salesOrderDetailOption = 'Sales Order Detail Report'
// const purchaseOptionDetailOption = 'Purchase Order Detail Report'
// const lineItemDetailOption = 'Line Item Detail Report'
// const RMADetailOption = 'RMA Detail Report'
// const shipmentDetailOption = 'Shipment Detail Report'

// fixture`Reports test`
//     .meta('TestRun', 'HS')
//     .before(async ctx => {
//         ctx.clientAuth = await adminClientSetup()
//         //wait 5 seconds to let everything get setup
//         await delay(5000)
//     })
//     .beforeEach(async t => {
//         await adminTestSetup()
//     })
//     .page(testConfig.adminAppUrl)

// test
//     .before(async t => {
//         const reportTemplate: ReportTemplate = await createReportTemplate(
//             t.fixtureCtx.clientAuth,
//             'BuyerLocation',
//             BUYER_LOCATION_HEADERS
//         )
//         t.ctx.reportName = reportTemplate.Name
//         t.ctx.reportID = reportTemplate.TemplateID
//         await adminTestSetup()
//     }).after(async t => {
//         await deleteCreatedTemplate(t.ctx.reportID, t.fixtureCtx.clientAuth)
//     })
//     ('Generate a Buyer Location Report | 20080', async t => {
//         await reportsProcessPage.selectReport(buyerLocationOption, t.ctx.reportName)
//         await t.expect(reportsProcessPage.iDOfTable.exists).ok()
//     })

// test
//     .before(async t => {
//         const reportTemplate: ReportTemplate = await createReportTemplate(
//             t.fixtureCtx.clientAuth,
//             'SalesOrderDetail',
//             SALES_ORDER_DETAIL_HEADERS
//         )
//         t.ctx.reportName = reportTemplate.Name
//         t.ctx.reportID = reportTemplate.TemplateID
//         await adminTestSetup()
//     }).after(async t => {
//         await deleteCreatedTemplate(t.ctx.reportID, t.fixtureCtx.clientAuth)
//     })
//     ('Generate a Sales Order Report | 20081', async t => {
//         await reportsProcessPage.selectReport(salesOrderDetailOption, t.ctx.reportName)
//         await t.expect(reportsProcessPage.iDOfTable.exists).ok()
//     })

// test
//     .before(async t => {
//         const reportTemplate = await createReportTemplate(
//             t.fixtureCtx.clientAuth,
//             'PurchaseOrderDetail',
//             PURCHASE_ORDER_DETAIL_HEADERS
//         )
//         t.ctx.reportName = reportTemplate.Name
//         t.ctx.reportID = reportTemplate.TemplateID
//         await adminTestSetup()
//     }).after(async t => {
//         await deleteCreatedTemplate(t.ctx.reportID, t.fixtureCtx.clientAuth)
//     })('Generate a Purchase Order Report | 20082', async t => {
//         await reportsProcessPage.selectReport(purchaseOptionDetailOption, t.ctx.reportName)
//         await t.expect(reportsProcessPage.iDOfTable.exists).ok()
//     })

// test
//     .before(async t => {
//         const reportTemplate = await createReportTemplate(
//             t.fixtureCtx.clientAuth,
//             'LineItemDetail',
//             LINE_ITEM_HEADERS
//         )
//         t.ctx.reportName = reportTemplate.Name
//         t.ctx.reportID = reportTemplate.TemplateID
//         await adminTestSetup()
//     }).after(async t => {
//         await deleteCreatedTemplate(t.ctx.reportID, t.fixtureCtx.clientAuth)
//     })
//     ('Generate a Line Item Detail Report | 20083', async t => {
//         await reportsProcessPage.selectReport(lineItemDetailOption, t.ctx.reportName)
//         await t.expect(reportsProcessPage.iDOfTable.exists).ok()
//     })

// test('Generate a RMA Detail Report | 21073', async t => {
//     const reportName = await reportsTemplatesPage.createRMAReport()
//     await reportsProcessPage.selectReport(RMADetailOption, t.ctx.reportName)
//     await t.expect(reportsProcessPage.iDOfTable.exists).ok()
// })

// test('Generate a Shipment Detail Report | 21074', async t => {
//     const reportName = await reportsTemplatesPage.createShipmentDetailReport()
//     await reportsProcessPage.selectReport(shipmentDetailOption, t.ctx.reportName)
//     await t.expect(reportsProcessPage.iDOfTable.exists).ok()
// })

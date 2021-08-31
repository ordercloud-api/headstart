import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'
import randomString from '../../helpers/random-string'
import { refreshPage } from '../../helpers/page-helper'
import mainResourcePage from './main-resource-page'
import adminHeaderPage from './admin-header-page'

class reportsTemplatesPage {
    templatesDropdown: Selector
    buyerLocationReport: Selector
    salesOrderReport: Selector
    PurchaseOrderReport: Selector
    lineItemDetailReport: Selector
    RMAReport: Selector
    shipmentDetailReport: Selector
    createNewTemplateButton: Selector
    availableToSuppliersSlider: Selector
    nameInput: Selector
    templateDescriptionInput: Selector
    createButton: Selector
    deleteButton: Selector
    backArrow: Selector
    reportListItem: Selector
    deleteConfirmButton: Selector


    constructor() {
        this.templatesDropdown = Selector('#parentresourcedropdown')
        this.buyerLocationReport = Selector('div').withExactText('Buyer Location Report')
        this.salesOrderReport = Selector('div').withExactText('Sales Order Detail Report')
        this.PurchaseOrderReport = Selector('div').withExactText('Purchase Order Detail Report')
        this.lineItemDetailReport = Selector('div').withExactText('Line Item Detail Report')
        this.RMAReport = Selector('div').withExactText('RMA Detail Report')
        this.shipmentDetailReport = Selector('div').withExactText('Shipment Detail Report')

        this.createNewTemplateButton = Selector('.btn.btn-block.brand-button--orange')
        this.availableToSuppliersSlider = Selector('.slider.round.cursor-pointer')
        this.nameInput = Selector('#Name')
        this.templateDescriptionInput = Selector('textarea')
        this.createButton = Selector('button').withExactText('Create')
        this.deleteButton = Selector('.btn.btn-link.text-danger')
        this.deleteConfirmButton = Selector('button').withText('Yes, Delete')
        this.backArrow = Selector('.icon-button.ripple.hover-btn')
        this.reportListItem = Selector('span')
    }

    async createAndDeleteBuyerLocationReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.buyerLocationReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }
    async createBuyerLocationReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.buyerLocationReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }

    async deleteBuyerLocationReport() {
        const reportName = `Automated Report_`
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }


    async createAndDeletSalesOrderReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.salesOrderReport)
        await t.click(this.createNewTemplateButton)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }

    async createSalesOrderReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.salesOrderReport)
        await t.click(this.createNewTemplateButton)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }

    async createAndDeletePurchaseOrderReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.PurchaseOrderReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }

    async createPurchaseOrderReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.PurchaseOrderReport)
        await t.click(this.createNewTemplateButton)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }
    async createAndDeleteLineItemDetailReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.lineItemDetailReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }
    async createLineItemDetailReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.lineItemDetailReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }

    async deleteLineItemDetailReport() {
        const reportName = `Automated Report_`
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }

    async createAndDeleteRMAReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.RMAReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }
    async createRMAReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.RMAReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }

    async deleteRMAReport() {
        const reportName = `Automated Report_`
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }

    async createAndDeleteShipmentDetailReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.shipmentDetailReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }
    async createShipmentDetailReport() {
        const reportName = `Automated Report_${randomString(5)}`
        const reportDescription = `Automated Report description text field ${randomString(20)}`
        await adminHeaderPage.selectReportsTemplates()
        await t.click(this.templatesDropdown)
        await t.click(this.shipmentDetailReport)
        await t.click(this.createNewTemplateButton)
        await t.click(this.availableToSuppliersSlider)
        await t.typeText(this.nameInput, reportName)
        await t.typeText(this.templateDescriptionInput, reportDescription)
        await t.click(this.createButton)
        await t.click(this.backArrow)
        await t.click(this.reportListItem.withText(reportName))
        return reportName
    }

    async deleteShipmentDetailReport() {
        const reportName = `Automated Report_`
        await t.click(this.reportListItem.withText(reportName))
        await t.click(this.deleteButton)
        await t.click(this.deleteConfirmButton)
    }
}

export default new reportsTemplatesPage()

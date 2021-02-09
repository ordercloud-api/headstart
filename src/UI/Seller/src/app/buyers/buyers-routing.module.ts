// core services
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { BuyerTableComponent } from './components/buyers/buyer-table/buyer-table.component'
import { BuyerUserTableComponent } from './components/users/buyer-user-table/buyer-user-table.component'
import { BuyerLocationTableComponent } from './components/locations/buyer-location-table/buyer-location-table.component'
import { BuyerPaymentTableComponent } from './components/payments/buyer-payment-table/buyer-payment-table.component'
import { BuyerApprovalTableComponent } from './components/approvals/buyer-approval-table/buyer-approval-table.component'
import { BuyerCategoryTableComponent } from './components/categories/buyer-category-table/buyer-category-table.component'
import { BuyerCatalogTableComponent } from './components/catalogs/buyer-catalog-table/buyer-catalog-table.component'

const routes: Routes = [
  { path: '', component: BuyerTableComponent },
  { path: 'new', component: BuyerTableComponent },
  { path: ':buyerID', component: BuyerTableComponent },
  { path: ':buyerID/users', component: BuyerUserTableComponent },
  { path: ':buyerID/users/new', component: BuyerUserTableComponent },
  { path: ':buyerID/users/:userID', component: BuyerUserTableComponent },
  { path: ':buyerID/locations', component: BuyerLocationTableComponent },
  { path: ':buyerID/locations/new', component: BuyerLocationTableComponent },
  {
    path: ':buyerID/locations/:locationID',
    component: BuyerLocationTableComponent,
  },
  { path: ':buyerID/payments', component: BuyerPaymentTableComponent },
  { path: ':buyerID/payments/new', component: BuyerPaymentTableComponent },
  {
    path: ':buyerID/payments/:paymentID',
    component: BuyerPaymentTableComponent,
  },
  { path: ':buyerID/approvals', component: BuyerApprovalTableComponent },
  { path: ':buyerID/approvals/new', component: BuyerApprovalTableComponent },
  {
    path: ':buyerID/approvals/:approvalID',
    component: BuyerApprovalTableComponent,
  },
  { path: ':buyerID/categories', component: BuyerCategoryTableComponent },
  { path: ':buyerID/categories/new', component: BuyerCategoryTableComponent },
  {
    path: ':buyerID/categories/:categoryID',
    component: BuyerCategoryTableComponent,
  },
  { path: ':buyerID/catalogs', component: BuyerCatalogTableComponent },
  { path: ':buyerID/catalogs/new', component: BuyerCatalogTableComponent },
  {
    path: ':buyerID/catalogs/:catalogID',
    component: BuyerCatalogTableComponent,
  },
]

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule],
})
export class BuyersRoutingModule {}

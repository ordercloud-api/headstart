import { UploadShipmentsComponent } from './orders/components/upload-shipments/upload-shipments.component'
// components
import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { HasTokenGuard as HasToken } from '@app-seller/shared'
import { HomeComponent } from '@app-seller/layout/home/home.component'
import { CanEditMySupplierGuard } from './shared/guards/can-edit-my-supplier/can-edit-my-supplier.guard'

const routes: Routes = [
  {
    path: '',
    canActivate: [HasToken],
    children: [
      { path: '', redirectTo: '/home', pathMatch: 'full' },
      { path: 'home', component: HomeComponent },
      {
        path: 'products',
        loadChildren: () =>
          import('./products/products.module').then((m) => m.ProductsModule),
      },
      // {
      //   path: 'kitproducts',
      //   loadChildren: () =>
      //     import('./kits/kits.module').then((m) => m.KitsModule),
      // },
      {
        path: 'promotions',
        loadChildren: () =>
          import('./promotions/promotions.module').then(
            (m) => m.PromotionsModule
          ),
      },
      {
        path: 'facets',
        loadChildren: () =>
          import('./facets/facets.module').then((m) => m.FacetsModule),
      },
      {
        path: 'orders',
        loadChildren: () =>
          import('./orders/orders.module').then((m) => m.OrdersModule),
      },
      {
        path: 'buyers',
        loadChildren: () =>
          import('./buyers/buyers.module').then((m) => m.BuyersModule),
      },
      {
        path: 'seller-users',
        loadChildren: () =>
          import('./seller-users/seller-users.module').then(
            (m) => m.SellerUsersModule
          ),
      },
      {
        path: 'suppliers',
        loadChildren: () =>
          import('./suppliers/suppliers.module').then((m) => m.SuppliersModule),
      },
      {
        path: 'my-supplier',
        canActivate: [CanEditMySupplierGuard],
        loadChildren: () =>
          import('./suppliers/suppliers.module').then((m) => m.SuppliersModule),
      },
      {
        path: 'reports',
        loadChildren: () =>
          import('./reports/reports.module').then((m) => m.ReportsModule),
      },
      {
        path: 'account',
        loadChildren: () =>
          import('./account/account.module').then((m) => m.AccountModule),
      },
      {
        path: 'support',
        loadChildren: () => 
          import('./support/support.module').then((m) => m.SupportModule)
      }
    ],
  },
]

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}

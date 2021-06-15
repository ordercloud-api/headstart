import { NgModule } from '@angular/core'
import { RouterModule, Routes } from '@angular/router'
import { HasTokenGuard } from './interceptors/has-token/has-token.guard'
import { IsProfiledUserGuard } from './interceptors/is-profiled-user/is-profiled-user.guard'
import { BaseResolve } from './resolves/base.resolve'
import { MeProductResolver } from './resolves/me.product.resolve'
import {
  MeListAddressResolver,
  MeListBuyerLocationResolver,
} from './resolves/me.resolve'
import { AddressListWrapperComponent } from './wrapper-components/address-list-wrapper.component'
import { CartWrapperComponent } from './wrapper-components/cart-wrapper.component'
import { CheckoutWrapperComponent } from './wrapper-components/checkout-wrapper.component'
import { ForgotPasswordWrapperComponent } from './wrapper-components/forgot-password-wrapper.component'
import { HomeWrapperComponent } from './wrapper-components/home-wrapper.component'
import { LocationListWrapperComponent } from './wrapper-components/location-list-wrapper.component'
import { LocationManagementWrapperComponent } from './wrapper-components/location-management-wrapper.component'
import { LoginWrapperComponent } from './wrapper-components/login-wrapper.component'
import { MeChangePasswordWrapperComponent } from './wrapper-components/me-change-password-wrapper.component'
import { OrderDetailWrapperComponent } from './wrapper-components/order-detail-wrapper.component'
import { OrderHistoryWrapperComponent } from './wrapper-components/order-history-wrapper-component'
import { PaymentListWrapperComponent } from './wrapper-components/payment-list-wrapper.component'
import { ProductDetailWrapperComponent } from './wrapper-components/product-detail-wrapper.component'
import { ProductListWrapperComponent } from './wrapper-components/product-list-wrapper.component'
import { ProfileWrapperComponent } from './wrapper-components/profile-wrapper.component'
import { RegisterWrapperComponent } from './wrapper-components/register-wrapper.component'
import { ResetPasswordWrapperComponent } from './wrapper-components/reset-password-wrapper.component'
import { SupplierListWrapperComponent } from './wrapper-components/supplier-list-wrapper.component'

const HeadstartRoutes: Routes = [
  { path: 'login', component: LoginWrapperComponent },
  { path: 'register', component: RegisterWrapperComponent },
  { path: 'forgot-password', component: ForgotPasswordWrapperComponent },
  { path: 'reset-password', component: ResetPasswordWrapperComponent },
  {
    path: '',
    canActivate: [HasTokenGuard],
    canActivateChild: [HasTokenGuard],
    resolve: {
      baseResolve: BaseResolve,
    },
    children: [
      { path: '', redirectTo: '/home', pathMatch: 'full' },
      { path: 'home', component: HomeWrapperComponent },
      { path: 'impersonation', redirectTo: '/home' },
      { path: 'sso', redirectTo: '/home' },

      { path: 'checkout', component: CheckoutWrapperComponent },
      { path: 'cart', component: CartWrapperComponent },
      { path: 'suppliers', component: SupplierListWrapperComponent },
      {
        path: 'products',
        component: ProductListWrapperComponent,
      },
      {
        path: 'products/:productID',
        resolve: {
          product: MeProductResolver,
        },
        children: [
          {
            path: '',
            component: ProductDetailWrapperComponent,
          },
        ],
      },
      {
        path: 'profile',
        canActivate: [IsProfiledUserGuard],
        children: [
          { path: '', component: ProfileWrapperComponent },
          {
            path: 'change-password',
            component: MeChangePasswordWrapperComponent,
          },
          {
            path: 'addresses',
            component: AddressListWrapperComponent,
            resolve: {
              addresses: MeListAddressResolver,
            },
          },
          {
            path: 'locations/:locationID',
            component: LocationManagementWrapperComponent,
          },
          {
            path: 'locations',
            component: LocationListWrapperComponent,
            resolve: {
              locations: MeListBuyerLocationResolver,
            },
          },
          { path: 'payment-methods', component: PaymentListWrapperComponent },
        ],
      },
      {
        path: 'orders',
        children: [
          { path: 'approve/:orderID', component: OrderDetailWrapperComponent, canActivate: [IsProfiledUserGuard] },
          { path: 'approve', component: OrderHistoryWrapperComponent, canActivate: [IsProfiledUserGuard] },
          {
            path: 'location/:locationFilter',
            component: OrderDetailWrapperComponent,
            canActivate: [IsProfiledUserGuard]
          },
          { path: 'location', component: OrderHistoryWrapperComponent, canActivate: [IsProfiledUserGuard] },
          { path: ':orderID', component: OrderDetailWrapperComponent, canActivate: [HasTokenGuard] },
          { path: '', component: OrderHistoryWrapperComponent, canActivate: [IsProfiledUserGuard] },
        ],
      },
    ],
  },
]

@NgModule({
  imports: [RouterModule.forRoot(HeadstartRoutes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }

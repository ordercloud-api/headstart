import {
  Component,
  Inject,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import {
  faShippingFast,
  faWindowClose,
  faPlus,
  faCog,
  faExclamationCircle,
  IconDefinition,
} from '@fortawesome/free-solid-svg-icons'
import {
  UntypedFormGroup,
  Validators,
  UntypedFormControl,
} from '@angular/forms'
import {
  LineItem,
  Shipment,
  SupplierAddresses,
  Address,
  Shipments,
  ShipmentItem,
  LineItems,
  Orders,
  Order,
  OrderDirection,
  ListPage,
} from 'ordercloud-javascript-sdk'
import { getProductSmallImageUrl } from '@app-seller/shared/services/assets/asset.helper'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { OrderService } from '@app-seller/orders/order.service'
import {
  CanChangeLineItemsOnOrderTo,
  NumberCanChangeTo,
  SellerOrderCanShip,
} from '@app-seller/orders/line-item-status.helper'
import {
  HeadStartSDK,
  HSLineItem,
  SuperHSShipment,
} from '@ordercloud/headstart-sdk'
import { flatten as _flatten } from 'lodash'
import { AppConfig } from '@app-seller/models/environment.types'
import { LineItemStatus, OrderType } from '@app-seller/models/order.types'
import { SELLER } from '@app-seller/models/user.types'

@Component({
  selector: 'app-order-shipments',
  templateUrl: './order-shipments.component.html',
  styleUrls: ['./order-shipments.component.scss'],
})
export class OrderShipmentsComponent implements OnChanges {
  faShippingFast = faShippingFast
  faPlus = faPlus
  faWindowClose = faWindowClose
  faCog = faCog // TO-DO - Use for edit button for Ship From address.
  faExclamationCircle = faExclamationCircle
  createShipment = false
  viewShipments = true
  editShipFromAddress = false // TO-DO - Use for editing Ship From address.
  shipmentForm: UntypedFormGroup
  // TODO: Get middleware route for listing super shipments
  superShipments: SuperHSShipment[] = []
  shipments: ListPage<Shipment>
  shipmentItems: ListPage<ShipmentItem>
  selectedShipment: Shipment
  supplierAddresses: ListPage<Address>
  lineItems: HSLineItem[]
  isSaving = false
  isSellerUser = false
  shipAllItems = false
  _order: Order
  @Input()
  orderDirection: OrderDirection
  @Input()
  set order(o: Order) {
    this._order = o
    this.handleSelectedOrderChange(o)
  }
  @Output()
  createOrViewShipmentEvent = new EventEmitter<boolean>()
  @Output()
  shipmentCreated = new EventEmitter<null>()

  constructor(
    private orderService: OrderService,
    private appAuthService: AppAuthService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {
    this.isSellerUser = this.appAuthService.getOrdercloudUserType() === SELLER
  }

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    if (this.orderDirection && this._order.ID) {
      this.superShipments = []
      if (this._order?.xp?.OrderType != OrderType.Quote) {
        await this.getShipments(this._order.ID)
        await this.getLineItems(this._order.ID)
      }
    }
  }

  async handleSelectedOrderChange(order: Order): Promise<void> {
    this.createShipment = false
    this.superShipments = []
  }

  setShipmentForm(): void {
    this.shipmentForm = new UntypedFormGroup({
      TrackingNumber: new UntypedFormControl(''),
      ShipDate: new UntypedFormControl(
        this.getCurrentDate(),
        Validators.required
      ),
      Cost: new UntypedFormControl(''),
      // TO-DO: Use below line of code when it's possible to POST a supplier's address ID
      // FromAddressID: new FormControl(''),
      Shipper: new UntypedFormControl(''),
      Service: new UntypedFormControl(''),
      Comment: new UntypedFormControl(''),
      LineItemData: new UntypedFormGroup({}),
    })
    const LineItemGroup = this.shipmentForm.get(
      'LineItemData'
    ) as UntypedFormGroup
    this.lineItems.forEach((item) => {
      LineItemGroup.addControl(
        item.ID,
        new UntypedFormGroup({
          Quantity: new UntypedFormControl(0),
          Comment: new UntypedFormControl(''),
        })
      )
    })
  }

  getCurrentDate(): string {
    // Get local date of user, then format to lead with year and leading 0's for single-digit months/days
    const date = new Date().toLocaleDateString()
    const newDate = date.split('/')
    if (newDate[0].length === 1) {
      newDate[0] = '0' + newDate[0]
    }
    if (newDate[1].length === 1) {
      newDate[1] = '0' + newDate[1]
    }
    return newDate[2] + '-' + newDate[0] + '-' + newDate[1]
  }

  toggleCreateShipment(): void {
    this.getSupplierAddresses()
    this.setShipmentForm()
    this.createShipment = !this.createShipment
    this.viewShipments = false
    this.createOrViewShipmentEvent.emit(this.createShipment)
  }

  canCreateShipment(): boolean {
    const unshippedItem = this.lineItems.find(
      (item) => item.Quantity > item.QuantityShipped
    )
    return unshippedItem ? true : false
  }

  isQuoteOrder(order: Order) {
    return this.orderService.isQuoteOrder(order)
  }

  toggleViewShipments(): void {
    this.setSelectedShipment(0)
    this.viewShipments = !this.viewShipments
    this.createShipment = false
    this.createOrViewShipmentEvent.emit(this.viewShipments)
  }

  // TO-DO - Use commented code when able to POST supplier Ship From addresses
  // toggleEditShipFromAddress() {
  //   this.editShipFromAddress = !this.editShipFromAddress;
  // }

  // handleUpdateShipFromAddress(addressID) {
  // }

  async getShipments(orderID: string): Promise<void> {
    // TODO: Have to use as any bc sdk doesn't recognize 'Shipper' as a valid sort
    const shipments = await Orders.ListShipments(this.orderDirection, orderID, {
      sortBy: ['!Shipper'],
    })
    shipments?.Items?.forEach(async (s: Shipment) => {
      const shipmentItems = await Shipments.ListItems(s.ID)
      this.superShipments.push({
        Shipment: s,
        ShipmentItems: shipmentItems.Items,
      })
    })
    this.shipments = shipments
  }

  async getLineItems(orderID: string): Promise<void> {
    let allOrderLineItems: HSLineItem[] = []
    const listOptions = {
      page: 1,
      pageSize: 100,
    }
    const lineItemsResponse = await LineItems.List(
      this.orderDirection,
      orderID,
      listOptions
    )
    allOrderLineItems = [
      ...allOrderLineItems,
      ...(lineItemsResponse.Items as HSLineItem[]),
    ]
    if (lineItemsResponse.Meta.TotalPages <= 1) {
      if (this.isSellerUser) {
        const sellerItems = lineItemsResponse.Items.filter(
          (li) => li.SupplierID === null
        )
        allOrderLineItems = [...(sellerItems as HSLineItem[])]
      }
      this.lineItems = allOrderLineItems
    } else {
      let lineItemRequests = []
      for (let page = 2; page <= lineItemsResponse.Meta.TotalPages; page++) {
        listOptions.page = page
        lineItemRequests = [
          ...lineItemRequests,
          LineItems.List(this.orderDirection, orderID, listOptions),
        ]
      }
      return await Promise.all(lineItemRequests).then((response) => {
        if (this.isSellerUser) {
          const sellerItems = response['Items'].filter(
            (li) => li.SupplierID === null
          )
          allOrderLineItems = [..._flatten(sellerItems)] as HSLineItem[]
        } else {
          allOrderLineItems = [
            ...allOrderLineItems,
            ..._flatten(response.map((r) => r.Items as HSLineItem[])),
          ]
        }
        this.lineItems = allOrderLineItems
      })
    }
  }

  async patchLineItems(): Promise<void> {
    const lineItemsToPatch = []
    const lineItemData = this.shipmentForm.value.LineItemData
    this.lineItems.forEach(async (li) => {
      if (
        lineItemData[li.ID]?.Quantity > 0 &&
        lineItemData[li.ID]?.Quantity === li.Quantity
      ) {
        lineItemsToPatch.push(
          LineItems.Patch(this.orderDirection, this._order?.ID, li.ID, {
            xp: { LineItemStatus: LineItemStatus.Complete },
          })
        )
      }
    })
    await Promise.all(lineItemsToPatch)
  }

  setSelectedShipment(i: number): void {
    this.selectedShipment = this.shipments.Items[i]
    this.getShipmentItems(this.selectedShipment.ID)
  }

  async getShipmentItems(shipmentID: string): Promise<void> {
    this.shipmentItems = await Shipments.ListItems(shipmentID)
  }

  getImageUrl(lineItem: LineItem): string {
    const product = lineItem.Product
    return getProductSmallImageUrl(product)
  }

  getCreateButtonAction(): string {
    return this.createShipment ? 'ADMIN.COMMON.CANCEL' : 'ADMIN.COMMON.CREATE'
  }

  // TO-DO - Use commented code for Ship From Address POST
  // getEditShipFromAddressButtonAction() {
  //   return this.editShipFromAddress ? 'ADMIN.COMMON.CANCEL' : 'Edit Ship From Address';
  // }

  getCreateButtonIcon(): IconDefinition {
    return this.createShipment ? faWindowClose : faPlus
  }

  getViewButtonIcon(): IconDefinition {
    return this.viewShipments ? faWindowClose : faShippingFast
  }

  // TO-DO - Use commented code for Ship From Address POST
  // getEditShipFromAddressButtonIcon() {
  //   return this.editShipFromAddress ? faWindowClose : faCog;
  // }

  async getSupplierAddresses(): Promise<void> {
    if (!this.supplierAddresses && !this.isSellerUser) {
      this.supplierAddresses = await SupplierAddresses.List(
        this._order?.ToCompanyID
      )
    }
  }

  getQuantityDropdown(lineItem: HSLineItem): number[] {
    const quantityAvailableToShip = NumberCanChangeTo(
      LineItemStatus.Complete,
      lineItem
    )
    const quantityList = []
    for (let i = 1; i <= quantityAvailableToShip; i++) {
      quantityList.push(i)
    }
    return quantityList
  }

  canShipLineItems(): boolean {
    return (
      this.lineItems &&
      CanChangeLineItemsOnOrderTo(LineItemStatus.Complete, this.lineItems) &&
      (SellerOrderCanShip(
        LineItemStatus.Complete,
        this.lineItems,
        this.isSellerUser
      ) ||
        !this.isSellerUser)
    )
  }

  toggleShipAllItems(): void {
    this.shipAllItems = !this.shipAllItems
    if (this.shipAllItems) {
      this.lineItems.forEach((item) => {
        const quantityToShip =
          item.Quantity -
          item.QuantityShipped -
          item.xp.StatusByQuantity['Backordered']
        if (quantityToShip) {
          this.shipmentForm.patchValue({
            LineItemData: {
              [item.ID]: {
                Quantity: quantityToShip,
              },
            },
          })
        }
      })
    }
  }

  async onSubmit(): Promise<void> {
    this.isSaving = true
    const shipment = this.shipmentForm.getRawValue()
    const shipDate = this.shipmentForm.value.ShipDate
    this.shipmentForm.value.ShipDate = null
    const superShipment = {
      Shipment: {
        TrackingNumber: shipment.TrackingNumber,
        DateShipped: shipment.ShipDate,
        Cost: shipment.Cost,
        Shipper: shipment.Shipper,
        xp: {
          Service: this.shipmentForm.value.Service,
          Comment: this.shipmentForm.value.Comment,
          BuyerID: this.isSellerUser ? this._order.FromCompanyID : null,
        },
      },
      ShipmentItems: this.lineItems
        .map((li) => {
          return {
            LineItemID: li.ID,
            OrderID: this._order?.ID,
            QuantityShipped: shipment.LineItemData[li.ID].Quantity,
            xp: {
              Comment: shipment.LineItemData[li.ID].Comment,
            },
          }
        })
        .filter((li) => li !== undefined),
    }
    await this.patchLineItems()
    await HeadStartSDK.Shipments.Create(superShipment)
    this.isSaving = false
    this.createOrViewShipmentEvent.emit(false)
    this.shipmentCreated.emit()
  }

  shouldDisableSave(shipment: UntypedFormGroup): boolean {
    if (shipment.value.TrackingNumber === '') return true
    if (shipment.value.ShipDate === '') return true
    if (shipment.value.Shipper === '') return true
    const lineItemData = this.shipmentForm.value.LineItemData
    const validQuantity = Object.values(lineItemData).find(
      (item: any) => item.Quantity > 0
    )
    if (!validQuantity) return true
    if (this.isSaving) return true
  }
}

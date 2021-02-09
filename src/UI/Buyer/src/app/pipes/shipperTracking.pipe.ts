import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'shipperTracking',
})
export class ShipperTrackingPipe implements PipeTransform {
  transform(trackingNumber: string, shipper: string): string {
    if (!trackingNumber || !shipper) {
      return
    }
    let shippingLink = ''
    switch (shipper.toLowerCase()) {
      case 'ups':
        shippingLink = `https://wwwapps.ups.com/WebTracking/track?track=yes&trackNums=${trackingNumber}`
        break
      case 'usps':
        shippingLink = `https://tools.usps.com/go/TrackConfirmAction?tLabels=${trackingNumber}`
        break
      case 'fedex':
        shippingLink = `https://www.fedex.com/apps/fedextrack/?tracknumbers=${trackingNumber}`
        break
    }
    return shippingLink
  }
}

@Pipe({
  name: 'shipperTrackingSupported',
})
export class ShipperTrackingSupportedPipe implements PipeTransform {
  transform(shipper: string): boolean {
    if (!shipper) {
      return false
    }
    const supportedShippers = ['ups', 'usps', 'fedex']
    return supportedShippers.includes(shipper.toLowerCase())
  }
}

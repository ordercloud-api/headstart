import { BuyerAddress } from '@ordercloud/angular-sdk'

export const getSuggestedAddresses = (ex): Array<BuyerAddress> => {
  for (const err of ex) {
    if (err.ErrorCode === 'blocked by web hook') {
      return err.Data?.Body?.SuggestedAddresses
    }
  }
  throw ex
}

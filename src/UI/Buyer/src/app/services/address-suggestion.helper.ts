import { BuyerAddress } from 'ordercloud-javascript-sdk'
export const getSuggestedAddresses = (ex): BuyerAddress[] => {
  const suggestions = ex?.response?.data?.Data?.SuggestedAddresses
  if (suggestions && Array.isArray(suggestions) && suggestions !== []) {
    return suggestions
  }
  // TODO - if suggestions === []
  throw ex
}

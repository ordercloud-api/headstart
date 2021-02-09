import { Selector } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class MyAddressesPage {
    addressBookH1: Selector

    constructor() {
        this.addressBookH1 = Selector('h1').withText(
            createRegExp('address book')
        )
    }
}

export default new MyAddressesPage()

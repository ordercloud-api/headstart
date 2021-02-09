import { Selector } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class MyCreditCardsPage {
    paymentMethodsH1: Selector

    constructor() {
        this.paymentMethodsH1 = Selector('h1').withText(
            createRegExp('payment methods')
        )
    }
}

export default new MyCreditCardsPage()

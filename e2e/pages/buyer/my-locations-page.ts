import { Selector } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class MyLocationsPage {
    locationsH1: Selector

    constructor() {
        this.locationsH1 = Selector('h1').withText(createRegExp('locations'))
    }
}

export default new MyLocationsPage()

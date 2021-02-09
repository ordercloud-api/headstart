import { Selector } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class MyProfilePage {
    accountDetailsH1: Selector

    constructor() {
        this.accountDetailsH1 = Selector('h1').withText(
            createRegExp('account details')
        )
    }
}

export default new MyProfilePage()

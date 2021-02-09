/* eslint-disable prettier/prettier */
import { Selector } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class Homepage {
	featuredProductsH3: Selector

	constructor() {
		this.featuredProductsH3 = Selector('h3').withText(createRegExp('featured products'))
	}
}

export default new Homepage()

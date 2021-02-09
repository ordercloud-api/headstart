import { ClientFunction, t } from 'testcafe'

export async function scrollIntoView(selectorString: string) {
	const scrollIntoView = ClientFunction(selectorString => {
		const element = document.querySelector(selectorString)
		element.scrollIntoView(true)
	})

	await scrollIntoView(selectorString)
}

//clicks to the left of an element, outside the element
export async function clickLeftOfElement(element: Selector) {
	let height = await element.offsetHeight
	let width = await element.offsetWidth
	//add 5 to width to get it over edge, then set it to negative to get it to go over the left side
	await t.click(element, {
		offsetX: (width + 5) * -1,
		offsetY: Math.ceil(height / 2),
	})
}

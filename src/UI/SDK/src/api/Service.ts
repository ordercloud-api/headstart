import { ListPage } from "../models";
import { flatten, range } from 'lodash'
import {ApprovalRule} from 'ordercloud-javascript-sdk'


export default class Services {

    constructor() {
        this.ListAll = this.ListAll.bind(this)
        this.BuildApproval = this.BuildApproval.bind(this)
    }
    
    /**
    * @param service relevant OrderCloud service of the OC resource you are listing
    * @param listFUnc List function being called
    * @param listArgs arguments to be passed into listFunction
    */

    public async ListAll<T = any>(
        service: any,
        listFunc: (...args: any) => Promise<ListPage<T>>,
        ...listArgs: any[]
    ): Promise<ListPage<T>> {
        // get or create filters obj if it doesnt exist
        listFunc = listFunc.bind(service)
        const hasFiltersObj = typeof listArgs[listArgs.length - 1] === 'object'
        const filtersObj = hasFiltersObj ? listArgs.pop() : {}

        // set page and pageSize
        filtersObj.page = 1
        filtersObj.pageSize = 100

        const result1 = await listFunc(...listArgs, filtersObj)
        const additionalPages = range(2, result1?.Meta.TotalPages + 1)

        const requests = additionalPages.map((page: number) =>
            listFunc(...listArgs, { ...filtersObj, page })
        )
        const results: ListPage<T>[] = await Promise.all(requests)
        // combine and flatten items for all list calls
        return {
            Items: flatten([result1, ...results].map((r) => r.Items)),
            Meta: result1.Meta,
        }
    }

    /**
    * @param locationID ID of the location that the approval rule applies to
    * @param orderThreshold Order total threshold for orders to be subject to this rule
    */
    public BuildApproval(locationID: string, orderThreshold?: number): ApprovalRule {
        return {
            ID: locationID,
            Name: locationID + ' General Location Approval Rule',
            Description: "General approval rule for location. " + 
                "Every order over a certain limit will require approval " + 
                "for the designated group of users.",
            ApprovingGroupID: `${locationID}-OrderApprover`,
            RuleExpression: "order.xp.ApprovalNeeded = '" + locationID + 
                "' & order.Total > " + (orderThreshold || 0),
            xp: {}
        }
    }
}
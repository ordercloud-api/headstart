import Buyers from './Buyers';
import SsoLogins from './SsoLogins';
import Catalogs from './Catalogs';
import Orders from './Orders';
import Payments from './Payments';
import Shipments from './Shipments';
import Suppliers from './Suppliers';
import Products from './Products';
import BuyerLocations from './BuyerLocations';
import Mes from './Mes';
import Reports from './Reports';
import TaxCategories from './TaxCategories';
import MePayments from './MePayments';
import MeCreditCardAuthorizations from './MeCreditCardAuthorizations';
import CreditCards from './CreditCards';
import ExchangeRates from './ExchangeRates';
import ValidatedAddresses from './ValidatedAddresses';
import Auth from './Auth';
import Tokens from './Tokens';
import Upload from './Upload';
import Assets from './Assets';
import Services from './Service';
import ApprovalRules from './ApprovalRules';
import RmAs from './RmAs';

export { HeadStartSDK }

const HeadStartSDK: HeadStartSDK = {
    Assets : new Assets(),
    Buyers : new Buyers(),
    SsoLogins : new SsoLogins(),
    Catalogs : new Catalogs(),
    RmAs: new RmAs(),
    Orders : new Orders(),
    Payments : new Payments(),
    Shipments : new Shipments(),
    Suppliers : new Suppliers(),
    Products : new Products(),
    BuyerLocations : new BuyerLocations(),
    Mes : new Mes(),
    Reports : new Reports(),
    TaxCategories : new TaxCategories(),
    MePayments : new MePayments(),
    MeCreditCardAuthorizations : new MeCreditCardAuthorizations(),
    CreditCards : new CreditCards(),
    ExchangeRates : new ExchangeRates(),
    ValidatedAddresses : new ValidatedAddresses(),
    Auth: Auth,
    Tokens: Tokens,
    Upload: new Upload(),
    Services: new Services(),
    ApprovalRules: new ApprovalRules()
}

interface HeadStartSDK {
    Assets : Assets,
    Buyers : Buyers,
    SsoLogins : SsoLogins,
    Catalogs : Catalogs,
    Orders : Orders,
    RmAs: RmAs,
    Payments : Payments,
    Shipments : Shipments,
    Suppliers : Suppliers,
    Products : Products,
    BuyerLocations : BuyerLocations,
    Mes : Mes,
    Reports : Reports,
    TaxCategories : TaxCategories,
    MePayments : MePayments,
    MeCreditCardAuthorizations : MeCreditCardAuthorizations,
    CreditCards : CreditCards,
    ExchangeRates : ExchangeRates,
    ValidatedAddresses : ValidatedAddresses,
    Auth: typeof Auth,
    Tokens: typeof Tokens,
    Upload: Upload,
    Services: Services,
    ApprovalRules: ApprovalRules
}

import AnytimeWebhooks from './AnytimeWebhooks';
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
import Avalaras from './Avalaras';
import MePayments from './MePayments';
import MeCreditCardAuthorizations from './MeCreditCardAuthorizations';
import CreditCards from './CreditCards';
import ExchangeRates from './ExchangeRates';
import ValidatedAddresses from './ValidatedAddresses';
import ChiliConfigs from './ChiliConfigs';
import ChiliSpecOptions from './ChiliSpecOptions';
import ChiliSpecs from './ChiliSpecs';
import ChiliTemplates from './ChiliTemplates';
import TecraDocuments from './TecraDocuments';
import TecraSpecs from './TecraSpecs';
import TecraFrames from './TecraFrames';
import TecraProofs from './TecraProofs';
import TecraPdFs from './TecraPdFs';
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
    AnytimeWebhooks : new AnytimeWebhooks(),
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
    Avalaras : new Avalaras(),
    MePayments : new MePayments(),
    MeCreditCardAuthorizations : new MeCreditCardAuthorizations(),
    CreditCards : new CreditCards(),
    ExchangeRates : new ExchangeRates(),
    ValidatedAddresses : new ValidatedAddresses(),
    ChiliConfigs : new ChiliConfigs(),
    ChiliSpecOptions : new ChiliSpecOptions(),
    ChiliSpecs : new ChiliSpecs(),
    ChiliTemplates : new ChiliTemplates(),
    TecraDocuments : new TecraDocuments(),
    TecraSpecs : new TecraSpecs(),
    TecraFrames : new TecraFrames(),
    TecraProofs : new TecraProofs(),
    TecraPdFs : new TecraPdFs(),
    Auth: new Auth(),
    Tokens: new Tokens(),
    Upload: new Upload(),
    Services: new Services(),
    ApprovalRules: new ApprovalRules()
}

interface HeadStartSDK {
    Assets : Assets,
    AnytimeWebhooks : AnytimeWebhooks,
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
    Avalaras : Avalaras,
    MePayments : MePayments,
    MeCreditCardAuthorizations : MeCreditCardAuthorizations,
    CreditCards : CreditCards,
    ExchangeRates : ExchangeRates,
    ValidatedAddresses : ValidatedAddresses,
    ChiliConfigs : ChiliConfigs,
    ChiliSpecOptions : ChiliSpecOptions,
    ChiliSpecs : ChiliSpecs,
    ChiliTemplates : ChiliTemplates,
    TecraDocuments : TecraDocuments,
    TecraSpecs : TecraSpecs,
    TecraFrames : TecraFrames,
    TecraProofs : TecraProofs,
    TecraPdFs : TecraPdFs,
    Auth: Auth,
    Tokens: Tokens,
    Upload: Upload,
    Services: Services,
    ApprovalRules: ApprovalRules
}

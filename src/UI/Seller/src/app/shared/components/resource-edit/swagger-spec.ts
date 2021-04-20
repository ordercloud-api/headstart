import { Validators } from "@angular/forms";
import { ValidateNoSpecialCharactersAndSpaces } from "@app-seller/validators/validators";

export const schemas = {
  AccessToken: {
    type: 'object',
    example: {
      access_token: '',
      expires_in: 0,
      token_type: '',
      refresh_token: '',
    },
    properties: {
      access_token: {
        type: 'string',
      },
      expires_in: {
        type: 'integer',
        format: 'int32',
      },
      token_type: {
        type: 'string',
      },
      refresh_token: {
        type: 'string',
      },
    },
  },
  AccessTokenBasic: {
    type: 'object',
    example: {
      access_token: '',
    },
    properties: {
      access_token: {
        type: 'string',
      },
    },
  },
  Address: {
    type: 'object',
    example: {
      ID: '',
      DateCreated: '2018-01-01T00:00:00-06:00',
      CompanyName: '',
      FirstName: '',
      LastName: '',
      Street1: '',
      Street2: '',
      City: '',
      State: '',
      Zip: '',
      Country: '',
      Phone: '',
      AddressName: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      CompanyName: {
        type: 'string',
        maxLength: 100,
      },
      FirstName: {
        type: 'string',
        maxLength: 100,
      },
      LastName: {
        type: 'string',
        maxLength: 100,
      },
      Street1: {
        type: 'string',
        maxLength: 100,
      },
      Street2: {
        type: 'string',
        maxLength: 100,
      },
      City: {
        type: 'string',
        maxLength: 100,
      },
      State: {
        type: 'string',
        maxLength: 100,
      },
      Zip: {
        type: 'string',
        maxLength: 100,
      },
      Country: {
        type: 'string',
        maxLength: 2,
      },
      Phone: {
        type: 'string',
        maxLength: 100,
      },
      AddressName: {
        type: 'string',
        maxLength: 100,
      },
      xp: {
        type: 'object',
      },
    },
  },
  AddressAssignment: {
    type: 'object',
    example: {
      AddressID: '',
      UserID: '',
      UserGroupID: '',
      IsShipping: false,
      IsBilling: false,
    },
    properties: {
      AddressID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      IsShipping: {
        type: 'boolean',
      },
      IsBilling: {
        type: 'boolean',
      },
    },
  },
  ApiClient: {
    type: 'object',
    example: {
      ID: '',
      ClientSecret: '',
      AccessTokenDuration: 0,
      Active: false,
      AppName: '',
      RefreshTokenDuration: 0,
      DefaultContextUserName: '',
      xp: {},
      AllowAnyBuyer: false,
      AllowAnySupplier: false,
      AllowSeller: false,
      IsAnonBuyer: false,
      AssignedBuyerCount: 0,
      AssignedSupplierCount: 0,
    },
    properties: {
      ID: {
        type: 'string',
        readOnly: true,
      },
      ClientSecret: {
        type: 'string',
      },
      AccessTokenDuration: {
        type: 'integer',
        format: 'int32',
      },
      Active: {
        type: 'boolean',
      },
      AppName: {
        type: 'string',
      },
      RefreshTokenDuration: {
        type: 'integer',
        format: 'int32',
      },
      DefaultContextUserName: {
        type: 'string',
      },
      xp: {
        type: 'object',
      },
      AllowAnyBuyer: {
        type: 'boolean',
      },
      AllowAnySupplier: {
        type: 'boolean',
      },
      AllowSeller: {
        type: 'boolean',
      },
      IsAnonBuyer: {
        type: 'boolean',
      },
      AssignedBuyerCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      AssignedSupplierCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
    },
  },
  ApiClientAssignment: {
    type: 'object',
    example: {
      ApiClientID: '',
      BuyerID: '',
      SupplierID: '',
    },
    properties: {
      ApiClientID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      SupplierID: {
        type: 'string',
      },
    },
  },
  ApprovalRule: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      ApprovingGroupID: '',
      RuleExpression: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
        validators: [ValidateNoSpecialCharactersAndSpaces]
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      ApprovingGroupID: {
        type: 'string',
        validators: [Validators.required]
      },
      RuleExpression: {
        type: 'string',
        maxLength: 400,
        validators: [Validators.required]
      },
      xp: {
        type: 'object',
      },
    },
  },
  Buyer: {
    type: 'object',
    example: {
      Name: '',
      DefaultCatalogID: '',
      Active: false,
      xp: {},
    },
    properties: {
      Name: {
        type: 'string',
        maxLength: 100,
      },
      DefaultCatalogID: {
        type: 'string',
      },
      Active: {
        type: 'boolean',
      },
      xp: {
        type: 'object',
      },
    },
  },
  BuyerAddress: {
    type: 'object',
    example: {
      ID: '',
      Shipping: false,
      Billing: false,
      Editable: false,
      DateCreated: '2018-01-01T00:00:00-06:00',
      CompanyName: '',
      FirstName: '',
      LastName: '',
      Street1: '',
      Street2: '',
      City: '',
      State: '',
      Zip: '',
      Country: '',
      Phone: '',
      AddressName: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
        readOnly: true,
      },
      Shipping: {
        type: 'boolean',
      },
      Billing: {
        type: 'boolean',
      },
      Editable: {
        type: 'boolean',
        readOnly: true,
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      CompanyName: {
        type: 'string',
        maxLength: 100,
      },
      FirstName: {
        type: 'string',
        maxLength: 100,
      },
      LastName: {
        type: 'string',
        maxLength: 100,
      },
      Street1: {
        type: 'string',
        maxLength: 100,
      },
      Street2: {
        type: 'string',
        maxLength: 100,
      },
      City: {
        type: 'string',
        maxLength: 100,
      },
      State: {
        type: 'string',
        maxLength: 100,
      },
      Zip: {
        type: 'string',
        maxLength: 100,
      },
      Country: {
        type: 'string',
        maxLength: 2,
      },
      Phone: {
        type: 'string',
        maxLength: 100,
      },
      AddressName: {
        type: 'string',
        maxLength: 100,
      },
      xp: {
        type: 'object',
      },
    },
  },
  BuyerCreditCard: {
    type: 'object',
    example: {
      ID: '',
      Editable: false,
      Token: '',
      DateCreated: '2018-01-01T00:00:00-06:00',
      CardType: '',
      PartialAccountNumber: '',
      CardholderName: '',
      ExpirationDate: '2018-01-01T00:00:00-06:00',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
        readOnly: true,
      },
      Editable: {
        type: 'boolean',
        readOnly: true,
      },
      Token: {
        type: 'string',
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      CardType: {
        type: 'string',
      },
      PartialAccountNumber: {
        type: 'string',
        maxLength: 5,
      },
      CardholderName: {
        type: 'string',
      },
      ExpirationDate: {
        type: 'string',
        format: 'date-time',
      },
      xp: {
        type: 'object',
      },
    },
  },
  BuyerProduct: {
    type: 'object',
    example: {
      PriceSchedule: {
        ID: '',
        Name: '',
        ApplyTax: false,
        ApplyShipping: false,
        MinQuantity: 0,
        MaxQuantity: 0,
        UseCumulativeQuantity: false,
        RestrictedQuantity: false,
        PriceBreaks: [
          {
            Quantity: 0,
            Price: 0,
          },
        ],
        xp: {},
      },
      ID: '',
      Name: '',
      Description: '',
      QuantityMultiplier: 0,
      ShipWeight: 0,
      ShipHeight: 0,
      ShipWidth: 0,
      ShipLength: 0,
      Active: false,
      SpecCount: 0,
      xp: {},
      VariantCount: 0,
      ShipFromAddressID: '',
      Inventory: {
        Enabled: false,
        NotificationPoint: 0,
        VariantLevelTracking: false,
        OrderCanExceed: false,
        QuantityAvailable: 0,
        LastUpdated: '2018-01-01T00:00:00-06:00',
      },
      DefaultSupplierID: '',
    },
    properties: {
      PriceSchedule: {
        allOf: [
          {
            $ref: '#/components/schemas/PriceSchedule',
          },
        ],
        readOnly: true,
      },
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      QuantityMultiplier: {
        type: 'integer',
        format: 'int32',
        default: 1,
        minimum: 1,
      },
      ShipWeight: {
        type: 'number',
        format: 'float',
      },
      ShipHeight: {
        type: 'number',
        format: 'float',
      },
      ShipWidth: {
        type: 'number',
        format: 'float',
      },
      ShipLength: {
        type: 'number',
        format: 'float',
      },
      Active: {
        type: 'boolean',
      },
      SpecCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      xp: {
        type: 'object',
      },
      VariantCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      ShipFromAddressID: {
        type: 'string',
      },
      Inventory: {
        allOf: [
          {
            $ref: '#/components/schemas/Inventory',
          },
        ],
      },
      DefaultSupplierID: {
        type: 'string',
      },
    },
  },
  BuyerSpec: {
    type: 'object',
    example: {
      Options: [
        {
          ID: '',
          Value: '',
          ListOrder: 1,
          IsOpenText: false,
          PriceMarkupType: 'NoMarkup',
          PriceMarkup: 0,
          xp: {},
        },
      ],
      ID: '',
      ListOrder: 1,
      Name: '',
      DefaultValue: '',
      Required: false,
      AllowOpenText: false,
      DefaultOptionID: '',
      DefinesVariant: false,
      xp: {},
    },
    properties: {
      Options: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SpecOption',
        },
        readOnly: true,
      },
      ID: {
        type: 'string',
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
      },
      Name: {
        type: 'string',
      },
      DefaultValue: {
        type: 'string',
        maxLength: 2000,
      },
      Required: {
        type: 'boolean',
      },
      AllowOpenText: {
        type: 'boolean',
      },
      DefaultOptionID: {
        type: 'string',
      },
      DefinesVariant: {
        type: 'boolean',
        description:
          "True if each unique combinations of this Spec's Options map to unique Product Variants/SKUs.",
      },
      xp: {
        type: 'object',
      },
    },
  },
  Catalog: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      Active: false,
      CategoryCount: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      Active: {
        type: 'boolean',
      },
      CategoryCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      xp: {
        type: 'object',
      },
    },
  },
  CatalogAssignment: {
    type: 'object',
    example: {
      CatalogID: '',
      BuyerID: '',
      ViewAllCategories: false,
      ViewAllProducts: false,
    },
    properties: {
      CatalogID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      ViewAllCategories: {
        type: 'boolean',
      },
      ViewAllProducts: {
        type: 'boolean',
      },
    },
  },
  Category: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      ListOrder: 1,
      Active: false,
      ParentID: '',
      ChildCount: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
        description:
          'Order that the category appears within its parent or catalog (if root level).',
        minimum: 0,
      },
      Active: {
        type: 'boolean',
        description:
          'If false, buyers cannot see this Category or any Categories or Products under it.',
      },
      ParentID: {
        type: 'string',
        description: 'ID of the parent category.',
      },
      ChildCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
        description:
          'Number of categories that are *immediate* children of this category.',
      },
      xp: {
        type: 'object',
      },
    },
  },
  CategoryAssignment: {
    type: 'object',
    example: {
      CategoryID: '',
      BuyerID: '',
      UserGroupID: '',
      Visible: false,
      ViewAllProducts: false,
    },
    properties: {
      CategoryID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      Visible: {
        type: 'boolean',
        description:
          'Optional. Set to null to inherit from parent category or catlog level.',
      },
      ViewAllProducts: {
        type: 'boolean',
        description:
          'Optional. Set to null to inherit from parent category or catlog level.',
      },
    },
  },
  CategoryProductAssignment: {
    type: 'object',
    example: {
      CategoryID: '',
      ProductID: '',
      ListOrder: 1,
    },
    properties: {
      CategoryID: {
        type: 'string',
      },
      ProductID: {
        type: 'string',
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
      },
    },
  },
  CostCenter: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      xp: {
        type: 'object',
      },
    },
  },
  CostCenterAssignment: {
    type: 'object',
    example: {
      CostCenterID: '',
      UserGroupID: '',
    },
    properties: {
      CostCenterID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
    },
  },
  CreditCard: {
    type: 'object',
    example: {
      ID: '',
      Token: '',
      DateCreated: '2018-01-01T00:00:00-06:00',
      CardType: '',
      PartialAccountNumber: '',
      CardholderName: '',
      ExpirationDate: '2018-01-01T00:00:00-06:00',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
        validators: [ValidateNoSpecialCharactersAndSpaces]
      },
      Token: {
        type: 'string',
      },
      DateCreated: {
        type: 'date',
        format: 'date-time',
        readOnly: true,
      },
      CardType: {
        type: 'string',
      },
      PartialAccountNumber: {
        type: 'string',
        maxLength: 5,
      },
      CardholderName: {
        type: 'string',
      },
      ExpirationDate: {
        type: 'date',
        format: 'date-time',
      },
      xp: {
        type: 'object',
      },
    },
  },
  CreditCardAssignment: {
    type: 'object',
    example: {
      CreditCardID: '',
      UserID: '',
      UserGroupID: '',
    },
    properties: {
      CreditCardID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
    },
  },
  ImpersonateTokenRequest: {
    type: 'object',
    example: {
      ClientID: '',
      Roles: ['DevCenter'],
    },
    properties: {
      ClientID: {
        type: 'string',
      },
      Roles: {
        type: 'array',
        enum: [
          'DevCenter',
          'DevCenterPasswordReset',
          'DevCenterValidateEmail',
          'GrantForAnyRole',
          'ApiClientAdmin',
          'ApiClientReader',
          'AddressAdmin',
          'AddressReader',
          'AdminAddressAdmin',
          'AdminAddressReader',
          'AdminUserAdmin',
          'AdminUserGroupAdmin',
          'AdminUserGroupReader',
          'AdminUserReader',
          'ApprovalRuleAdmin',
          'ApprovalRuleReader',
          'BuyerAdmin',
          'BuyerImpersonation',
          'BuyerReader',
          'BuyerUserAdmin',
          'BuyerUserReader',
          'CatalogAdmin',
          'CatalogReader',
          'CategoryAdmin',
          'CategoryReader',
          'CostCenterAdmin',
          'CostCenterReader',
          'CreditCardAdmin',
          'CreditCardReader',
          'FullAccess',
          'IncrementorAdmin',
          'IncrementorReader',
          'InventoryAdmin',
          'MeAddressAdmin',
          'MeAdmin',
          'MeCreditCardAdmin',
          'MessageConfigAssignmentAdmin',
          'MeXpAdmin',
          'OrderAdmin',
          'OrderReader',
          'OverrideShipping',
          'OverrideTax',
          'OverrideUnitPrice',
          'PasswordReset',
          'PriceScheduleAdmin',
          'PriceScheduleReader',
          'ProductAdmin',
          'ProductAssignmentAdmin',
          'ProductFacetAdmin',
          'ProductFacetReader',
          'ProductReader',
          'PromotionAdmin',
          'PromotionReader',
          'SecurityProfileAdmin',
          'SecurityProfileReader',
          'SetSecurityProfile',
          'ShipmentAdmin',
          'ShipmentReader',
          'Shopper',
          'SpendingAccountAdmin',
          'SpendingAccountReader',
          'SupplierAddressAdmin',
          'SupplierAddressReader',
          'SupplierAdmin',
          'SupplierReader',
          'SupplierUserAdmin',
          'SupplierUserGroupAdmin',
          'SupplierUserGroupReader',
          'SupplierUserReader',
          'UnsubmittedOrderReader',
          'UserGroupAdmin',
          'UserGroupReader',
          'OpenIDConnectReader',
          'OpenIDConnectAdmin',
          'MessageSenderReader',
          'MessageSenderAdmin',
          'XpIndexAdmin',
          'WebhookReader',
          'WebhookAdmin',
        ],
        items: {
          type: 'string',
        },
      },
    },
  },
  ImpersonationConfig: {
    type: 'object',
    example: {
      ID: '',
      ImpersonationBuyerID: '',
      ImpersonationGroupID: '',
      ImpersonationUserID: '',
      BuyerID: '',
      GroupID: '',
      UserID: '',
      SecurityProfileID: '',
      ClientID: '',
    },
    properties: {
      ID: {
        type: 'string',
      },
      ImpersonationBuyerID: {
        type: 'string',
      },
      ImpersonationGroupID: {
        type: 'string',
      },
      ImpersonationUserID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      GroupID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
      SecurityProfileID: {
        type: 'string',
      },
      ClientID: {
        type: 'string',
      },
    },
  },
  Incrementor: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      LastNumber: 0,
      LeftPaddingCount: 0,
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      LastNumber: {
        type: 'integer',
        format: 'int32',
      },
      LeftPaddingCount: {
        type: 'integer',
        format: 'int32',
      },
    },
  },
  Inventory: {
    type: 'object',
    example: {
      Enabled: false,
      NotificationPoint: 0,
      VariantLevelTracking: false,
      OrderCanExceed: false,
      QuantityAvailable: 0,
      LastUpdated: '2018-01-01T00:00:00-06:00',
    },
    properties: {
      Enabled: {
        type: 'boolean',
      },
      NotificationPoint: {
        type: 'integer',
        format: 'int32',
      },
      VariantLevelTracking: {
        type: 'boolean',
      },
      OrderCanExceed: {
        type: 'boolean',
      },
      QuantityAvailable: {
        type: 'integer',
        format: 'int32',
        description: 'Automatically decrements on order submit.',
      },
      LastUpdated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
    },
  },
  LineItem: {
    type: 'object',
    example: {
      ID: '',
      ProductID: '',
      Quantity: 0,
      DateAdded: '2018-01-01T00:00:00-06:00',
      QuantityShipped: 0,
      UnitPrice: 0,
      LineTotal: 0,
      CostCenter: '',
      DateNeeded: '2018-01-01T00:00:00-06:00',
      ShippingAccount: '',
      ShippingAddressID: '',
      ShipFromAddressID: '',
      Product: {
        ID: '',
        Name: '',
        Description: '',
        QuantityMultiplier: 0,
        ShipWeight: 0,
        ShipHeight: 0,
        ShipWidth: 0,
        ShipLength: 0,
        xp: {},
      },
      Variant: {
        ID: '',
        Name: '',
        Description: '',
        ShipWeight: 0,
        ShipHeight: 0,
        ShipWidth: 0,
        ShipLength: 0,
        xp: {},
      },
      ShippingAddress: {
        ID: '',
        DateCreated: '2018-01-01T00:00:00-06:00',
        CompanyName: '',
        FirstName: '',
        LastName: '',
        Street1: '',
        Street2: '',
        City: '',
        State: '',
        Zip: '',
        Country: '',
        Phone: '',
        AddressName: '',
        xp: {},
      },
      ShipFromAddress: {
        ID: '',
        DateCreated: '2018-01-01T00:00:00-06:00',
        CompanyName: '',
        FirstName: '',
        LastName: '',
        Street1: '',
        Street2: '',
        City: '',
        State: '',
        Zip: '',
        Country: '',
        Phone: '',
        AddressName: '',
        xp: {},
      },
      SupplierID: '',
      Specs: [
        {
          SpecID: '',
          Name: '',
          OptionID: '',
          Value: '',
        },
      ],
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      ProductID: {
        type: 'string',
      },
      Quantity: {
        type: 'integer',
        format: 'int32',
        default: 1,
        minimum: 1,
      },
      DateAdded: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      QuantityShipped: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      UnitPrice: {
        type: 'number',
        format: 'float',
      },
      LineTotal: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      CostCenter: {
        type: 'string',
      },
      DateNeeded: {
        type: 'string',
        format: 'date-time',
      },
      ShippingAccount: {
        type: 'string',
      },
      ShippingAddressID: {
        type: 'string',
      },
      ShipFromAddressID: {
        type: 'string',
      },
      Product: {
        allOf: [
          {
            $ref: '#/components/schemas/LineItemProduct',
          },
        ],
        readOnly: true,
      },
      Variant: {
        allOf: [
          {
            $ref: '#/components/schemas/LineItemVariant',
          },
        ],
        readOnly: true,
      },
      ShippingAddress: {
        allOf: [
          {
            $ref: '#/components/schemas/Address',
          },
        ],
        readOnly: true,
      },
      ShipFromAddress: {
        allOf: [
          {
            $ref: '#/components/schemas/Address',
          },
        ],
        readOnly: true,
      },
      SupplierID: {
        type: 'string',
        readOnly: true,
      },
      Specs: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/LineItemSpec',
        },
      },
      xp: {
        type: 'object',
      },
    },
  },
  LineItemProduct: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      QuantityMultiplier: 0,
      ShipWeight: 0,
      ShipHeight: 0,
      ShipWidth: 0,
      ShipLength: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
      },
      Description: {
        type: 'string',
      },
      QuantityMultiplier: {
        type: 'integer',
        format: 'int32',
      },
      ShipWeight: {
        type: 'number',
        format: 'float',
      },
      ShipHeight: {
        type: 'number',
        format: 'float',
      },
      ShipWidth: {
        type: 'number',
        format: 'float',
      },
      ShipLength: {
        type: 'number',
        format: 'float',
      },
      xp: {
        type: 'object',
      },
    },
  },
  LineItemSpec: {
    type: 'object',
    example: {
      SpecID: '',
      Name: '',
      OptionID: '',
      Value: '',
    },
    properties: {
      SpecID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        readOnly: true,
      },
      OptionID: {
        type: 'string',
      },
      Value: {
        type: 'string',
        maxLength: 2000,
      },
    },
  },
  LineItemVariant: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      ShipWeight: 0,
      ShipHeight: 0,
      ShipWidth: 0,
      ShipLength: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
      },
      Description: {
        type: 'string',
      },
      ShipWeight: {
        type: 'number',
        format: 'float',
      },
      ShipHeight: {
        type: 'number',
        format: 'float',
      },
      ShipWidth: {
        type: 'number',
        format: 'float',
      },
      ShipLength: {
        type: 'number',
        format: 'float',
      },
      xp: {
        type: 'object',
      },
    },
  },
  ListAddress: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Address',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListAddressAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/AddressAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListApiClient: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ApiClient',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListApiClientAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ApiClientAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListApprovalRule: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ApprovalRule',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListBuyer: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Buyer',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListBuyerAddress: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/BuyerAddress',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListBuyerCreditCard: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/BuyerCreditCard',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListBuyerProduct: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/BuyerProduct',
        },
      },
      Meta: {
        $ref: '#/components/schemas/MetaWithFacets',
      },
    },
  },
  ListBuyerSpec: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/BuyerSpec',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCatalog: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Catalog',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCatalogAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CatalogAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCategory: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Category',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCategoryAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CategoryAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCategoryProductAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CategoryProductAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCostCenter: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CostCenter',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCostCenterAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CostCenterAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCreditCard: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CreditCard',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListCreditCardAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/CreditCardAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListFacet: {
    type: 'object',
    properties: {
      Name: {
        type: 'string',
      },
      XpPath: {
        type: 'string',
      },
      Values: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ListFacetValue',
        },
      },
      xp: {
        type: 'object',
      },
    },
  },
  ListFacetValue: {
    type: 'object',
    properties: {
      Value: {
        type: 'string',
      },
      Count: {
        type: 'integer',
        format: 'int32',
      },
    },
  },
  ListImpersonationConfig: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ImpersonationConfig',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListIncrementor: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Incrementor',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListLineItem: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/LineItem',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListMessageCCListenerAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/MessageCCListenerAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListMessageSender: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/MessageSender',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListMessageSenderAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/MessageSenderAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListOpenIdConnect: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/OpenIdConnect',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListOrder: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Order',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListOrderApproval: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/OrderApproval',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListOrderPromotion: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/OrderPromotion',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListPayment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Payment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListPriceSchedule: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/PriceSchedule',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListProduct: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Product',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListProductAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ProductAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListProductCatalogAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ProductCatalogAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListProductFacet: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ProductFacet',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListPromotion: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Promotion',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListPromotionAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/PromotionAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSecurityProfile: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SecurityProfile',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSecurityProfileAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SecurityProfileAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListShipment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Shipment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListShipmentItem: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ShipmentItem',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSpec: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Spec',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSpecOption: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SpecOption',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSpecProductAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SpecProductAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSpendingAccount: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SpendingAccount',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSpendingAccountAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/SpendingAccountAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListSupplier: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Supplier',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListUser: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/User',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListUserGroup: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/UserGroup',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListUserGroupAssignment: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/UserGroupAssignment',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListVariant: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Variant',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListWebhook: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/Webhook',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  ListXpIndex: {
    type: 'object',
    properties: {
      Items: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/XpIndex',
        },
      },
      Meta: {
        $ref: '#/components/schemas/Meta',
      },
    },
  },
  MeBuyer: {
    type: 'object',
    example: {
      ID: '',
      DefaultCatalogID: '',
    },
    properties: {
      ID: {
        type: 'string',
        readOnly: true,
      },
      DefaultCatalogID: {
        type: 'string',
        readOnly: true,
      },
    },
  },
  MessageCCListenerAssignment: {
    type: 'object',
    example: {
      MessageSenderAssignment: {
        MessageSenderID: '',
        BuyerID: '',
        SupplierID: '',
        UserGroupID: '',
        MessageConfigName: '',
        MessageConfigDescription: '',
      },
      MessageConfigName: '',
      MessageConfigDescription: '',
      MessageType: 'OrderDeclined',
      BuyerID: '',
      SupplierID: '',
      UserGroupID: '',
      UserID: '',
    },
    properties: {
      MessageSenderAssignment: {
        allOf: [
          {
            $ref: '#/components/schemas/MessageSenderAssignment',
          },
        ],
      },
      MessageConfigName: {
        type: 'string',
        readOnly: true,
      },
      MessageConfigDescription: {
        type: 'string',
        readOnly: true,
      },
      MessageType: {
        enum: [
          'OrderDeclined',
          'OrderSubmitted',
          'ShipmentCreated',
          'ForgottenPassword',
          'OrderSubmittedForYourApproval',
          'OrderSubmittedForApproval',
          'OrderApproved',
          'OrderSubmittedForYourApprovalHasBeenApproved',
          'OrderSubmittedForYourApprovalHasBeenDeclined',
          'NewUserInvitation',
        ],
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      SupplierID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
    },
  },
  MessageSender: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      MessageTypes: ['OrderDeclined'],
      Description: '',
      URL: '',
      ElevatedRoles: ['DevCenter'],
      SharedKey: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
      },
      MessageTypes: {
        type: 'array',
        enum: [
          'OrderDeclined',
          'OrderSubmitted',
          'ShipmentCreated',
          'ForgottenPassword',
          'OrderSubmittedForYourApproval',
          'OrderSubmittedForApproval',
          'OrderApproved',
          'OrderSubmittedForYourApprovalHasBeenApproved',
          'OrderSubmittedForYourApprovalHasBeenDeclined',
          'NewUserInvitation',
        ],
        items: {
          type: 'string',
        },
      },
      Description: {
        type: 'string',
      },
      URL: {
        type: 'string',
      },
      ElevatedRoles: {
        type: 'array',
        enum: [
          'DevCenter',
          'DevCenterPasswordReset',
          'DevCenterValidateEmail',
          'GrantForAnyRole',
          'ApiClientAdmin',
          'ApiClientReader',
          'AddressAdmin',
          'AddressReader',
          'AdminAddressAdmin',
          'AdminAddressReader',
          'AdminUserAdmin',
          'AdminUserGroupAdmin',
          'AdminUserGroupReader',
          'AdminUserReader',
          'ApprovalRuleAdmin',
          'ApprovalRuleReader',
          'BuyerAdmin',
          'BuyerImpersonation',
          'BuyerReader',
          'BuyerUserAdmin',
          'BuyerUserReader',
          'CatalogAdmin',
          'CatalogReader',
          'CategoryAdmin',
          'CategoryReader',
          'CostCenterAdmin',
          'CostCenterReader',
          'CreditCardAdmin',
          'CreditCardReader',
          'FullAccess',
          'IncrementorAdmin',
          'IncrementorReader',
          'InventoryAdmin',
          'MeAddressAdmin',
          'MeAdmin',
          'MeCreditCardAdmin',
          'MessageConfigAssignmentAdmin',
          'MeXpAdmin',
          'OrderAdmin',
          'OrderReader',
          'OverrideShipping',
          'OverrideTax',
          'OverrideUnitPrice',
          'PasswordReset',
          'PriceScheduleAdmin',
          'PriceScheduleReader',
          'ProductAdmin',
          'ProductAssignmentAdmin',
          'ProductFacetAdmin',
          'ProductFacetReader',
          'ProductReader',
          'PromotionAdmin',
          'PromotionReader',
          'SecurityProfileAdmin',
          'SecurityProfileReader',
          'SetSecurityProfile',
          'ShipmentAdmin',
          'ShipmentReader',
          'Shopper',
          'SpendingAccountAdmin',
          'SpendingAccountReader',
          'SupplierAddressAdmin',
          'SupplierAddressReader',
          'SupplierAdmin',
          'SupplierReader',
          'SupplierUserAdmin',
          'SupplierUserGroupAdmin',
          'SupplierUserGroupReader',
          'SupplierUserReader',
          'UnsubmittedOrderReader',
          'UserGroupAdmin',
          'UserGroupReader',
          'OpenIDConnectReader',
          'OpenIDConnectAdmin',
          'MessageSenderReader',
          'MessageSenderAdmin',
          'XpIndexAdmin',
          'WebhookReader',
          'WebhookAdmin',
        ],
        items: {
          type: 'string',
        },
      },
      SharedKey: {
        type: 'string',
      },
      xp: {
        type: 'object',
      },
    },
  },
  MessageSenderAssignment: {
    type: 'object',
    example: {
      MessageSenderID: '',
      BuyerID: '',
      SupplierID: '',
      UserGroupID: '',
      MessageConfigName: '',
      MessageConfigDescription: '',
    },
    properties: {
      MessageSenderID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      SupplierID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      MessageConfigName: {
        type: 'string',
        readOnly: true,
      },
      MessageConfigDescription: {
        type: 'string',
        readOnly: true,
      },
    },
  },
  MeSupplier: {
    type: 'object',
    example: {
      ID: '',
    },
    properties: {
      ID: {
        type: 'string',
        readOnly: true,
      },
    },
  },
  Meta: {
    type: 'object',
    properties: {
      Page: {
        type: 'integer',
        format: 'int32',
      },
      PageSize: {
        type: 'integer',
        format: 'int32',
      },
      TotalCount: {
        type: 'integer',
        format: 'int32',
      },
      TotalPages: {
        type: 'integer',
        format: 'int32',
      },
      ItemRange: {
        type: 'array',
        items: {
          type: 'integer',
          format: 'int32',
        },
      },
    },
  },
  MetaWithFacets: {
    type: 'object',
    properties: {
      Facets: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/ListFacet',
        },
      },
      Page: {
        type: 'integer',
        format: 'int32',
      },
      PageSize: {
        type: 'integer',
        format: 'int32',
      },
      TotalCount: {
        type: 'integer',
        format: 'int32',
      },
      TotalPages: {
        type: 'integer',
        format: 'int32',
      },
      ItemRange: {
        type: 'array',
        items: {
          type: 'integer',
          format: 'int32',
        },
      },
    },
  },
  MeUser: {
    type: 'object',
    example: {
      Buyer: {
        ID: '',
        DefaultCatalogID: '',
      },
      Supplier: {
        ID: '',
      },
      ID: '',
      Username: '',
      Password: '',
      FirstName: '',
      LastName: '',
      Email: '',
      Phone: '',
      TermsAccepted: '2018-01-01T00:00:00-06:00',
      Active: false,
      xp: {},
      AvailableRoles: [''],
      DateCreated: '2018-01-01T00:00:00-06:00',
      PasswordLastSetDate: '2018-01-01T00:00:00-06:00',
    },
    properties: {
      Buyer: {
        allOf: [
          {
            $ref: '#/components/schemas/MeBuyer',
          },
        ],
        readOnly: true,
      },
      Supplier: {
        allOf: [
          {
            $ref: '#/components/schemas/MeSupplier',
          },
        ],
        readOnly: true,
      },
      ID: {
        type: 'string',
      },
      Username: {
        type: 'string',
        maxLength: 100,
      },
      Password: {
        type: 'string',
        format: 'password',
      },
      FirstName: {
        type: 'string',
        maxLength: 100,
      },
      LastName: {
        type: 'string',
        maxLength: 100,
      },
      Email: {
        type: 'string',
        maxLength: 200,
      },
      Phone: {
        type: 'string',
        maxLength: 100,
      },
      TermsAccepted: {
        type: 'string',
        format: 'date-time',
      },
      Active: {
        type: 'boolean',
      },
      xp: {
        type: 'object',
      },
      AvailableRoles: {
        type: 'array',
        items: {
          type: 'string',
        },
        readOnly: true,
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      PasswordLastSetDate: {
        type: 'string',
        format: 'password',
        readOnly: true,
      },
    },
  },
  OpenIdConnect: {
    type: 'object',
    example: {
      ID: '',
      OrderCloudApiClientID: '',
      ConnectClientID: '',
      ConnectClientSecret: '',
      AppStartUrl: '',
      AuthorizationEndpoint: '',
      TokenEndpoint: '',
    },
    properties: {
      ID: {
        type: 'string',
        description:
          'ID of this OpenID Connect configuration object. Each object allows authentication to one Ordercloud ApiClient through one Identity Providing Party.',
      },
      OrderCloudApiClientID: {
        type: 'string',
        description: 'An ID that references an Ordercloud ApiClient.',
      },
      ConnectClientID: {
        type: 'string',
        description:
          'An app ID from the Identity Provider that is required to get JWT tokens.',
      },
      ConnectClientSecret: {
        type: 'string',
        description:
          'A secret string from the Identity Provider that grants access to get JWT tokens.',
      },
      AppStartUrl: {
        type: 'string',
        description:
          'A URL on your front-end ordering site where users will be redirected after they authenticate through the Identity Provider. The string "{token}" will be replaced with a valid Ordercloud JWT.',
      },
      AuthorizationEndpoint: {
        type: 'string',
        description:
          'A publicly known URL from the Identity Provider that redirects to a resource where users enter personal credentials.',
      },
      TokenEndpoint: {
        type: 'string',
        description:
          'A publicly known URL from the Identity Provider where agents can get JWT tokens.',
      },
    },
  },
  Order: {
    type: 'object',
    example: {
      ID: '',
      FromUser: {
        ID: '',
        Username: '',
        Password: '',
        FirstName: '',
        LastName: '',
        Email: '',
        Phone: '',
        TermsAccepted: '2018-01-01T00:00:00-06:00',
        Active: false,
        xp: {},
        AvailableRoles: [''],
        DateCreated: '2018-01-01T00:00:00-06:00',
        PasswordLastSetDate: '2018-01-01T00:00:00-06:00',
      },
      FromCompanyID: '',
      FromUserID: '',
      BillingAddressID: '',
      BillingAddress: {
        ID: '',
        DateCreated: '2018-01-01T00:00:00-06:00',
        CompanyName: '',
        FirstName: '',
        LastName: '',
        Street1: '',
        Street2: '',
        City: '',
        State: '',
        Zip: '',
        Country: '',
        Phone: '',
        AddressName: '',
        xp: {},
      },
      ShippingAddressID: '',
      Comments: '',
      LineItemCount: 0,
      Status: 'Unsubmitted',
      DateCreated: '2018-01-01T00:00:00-06:00',
      DateSubmitted: '2018-01-01T00:00:00-06:00',
      DateApproved: '2018-01-01T00:00:00-06:00',
      DateDeclined: '2018-01-01T00:00:00-06:00',
      DateCanceled: '2018-01-01T00:00:00-06:00',
      DateCompleted: '2018-01-01T00:00:00-06:00',
      Subtotal: 0,
      ShippingCost: 0,
      TaxCost: 0,
      PromotionDiscount: 0,
      Total: 0,
      IsSubmitted: false,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      FromUser: {
        allOf: [
          {
            $ref: '#/components/schemas/User',
          },
        ],
        readOnly: true,
      },
      FromCompanyID: {
        type: 'string',
      },
      FromUserID: {
        type: 'string',
      },
      BillingAddressID: {
        type: 'string',
      },
      BillingAddress: {
        allOf: [
          {
            $ref: '#/components/schemas/Address',
          },
        ],
        readOnly: true,
      },
      ShippingAddressID: {
        type: 'string',
        description:
          'ID of the Shipping Address for all LineItems on the Order. Null when there are multiple Shipping Addresses involved.',
      },
      Comments: {
        type: 'string',
        maxLength: 2000,
      },
      LineItemCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      Status: {
        enum: [
          'Unsubmitted',
          'AwaitingApproval',
          'Declined',
          'Open',
          'Completed',
          'Canceled',
        ],
        type: 'string',
        readOnly: true,
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateSubmitted: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateApproved: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateDeclined: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateCanceled: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateCompleted: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      Subtotal: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      ShippingCost: {
        type: 'number',
        format: 'float',
      },
      TaxCost: {
        type: 'number',
        format: 'float',
      },
      PromotionDiscount: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      Total: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      IsSubmitted: {
        type: 'boolean',
        readOnly: true,
        description:
          'True if this Order has been passed from the Buyer to the Seller.',
      },
      xp: {
        type: 'object',
      },
    },
  },
  OrderApproval: {
    type: 'object',
    example: {
      ApprovalRuleID: '',
      ApprovingGroupID: '',
      Status: 'Pending',
      AllowResubmit: false,
      DateCreated: '2018-01-01T00:00:00-06:00',
      DateCompleted: '2018-01-01T00:00:00-06:00',
      Approver: {
        ID: '',
        Username: '',
        Password: '',
        FirstName: '',
        LastName: '',
        Email: '',
        Phone: '',
        TermsAccepted: '2018-01-01T00:00:00-06:00',
        Active: false,
        xp: {},
        AvailableRoles: [''],
        DateCreated: '2018-01-01T00:00:00-06:00',
        PasswordLastSetDate: '2018-01-01T00:00:00-06:00',
      },
      Comments: '',
    },
    properties: {
      ApprovalRuleID: {
        type: 'string',
        readOnly: true,
      },
      ApprovingGroupID: {
        type: 'string',
        readOnly: true,
      },
      Status: {
        enum: ['Pending', 'Approved', 'Declined'],
        type: 'string',
        readOnly: true,
      },
      AllowResubmit: {
        type: 'boolean',
        readOnly: true,
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      DateCompleted: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      Approver: {
        allOf: [
          {
            $ref: '#/components/schemas/User',
          },
        ],
        readOnly: true,
      },
      Comments: {
        type: 'string',
        readOnly: true,
      },
    },
  },
  OrderApprovalInfo: {
    type: 'object',
    example: {
      Comments: '',
      AllowResubmit: false,
    },
    properties: {
      Comments: {
        type: 'string',
        maxLength: 2000,
      },
      AllowResubmit: {
        type: 'boolean',
      },
    },
  },
  OrderPromotion: {
    type: 'object',
    example: {
      Amount: 0,
      ID: '',
      Code: '',
      Name: '',
      RedemptionLimit: 0,
      RedemptionLimitPerUser: 0,
      RedemptionCount: 0,
      Description: '',
      FinePrint: '',
      StartDate: '2018-01-01T00:00:00-06:00',
      ExpirationDate: '2018-01-01T00:00:00-06:00',
      EligibleExpression: '',
      ValueExpression: '',
      CanCombine: false,
      AllowAllBuyers: false,
      xp: {},
    },
    properties: {
      Amount: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      ID: {
        type: 'string',
      },
      Code: {
        type: 'string',
        description:
          'Must be unique. Entered by buyer when adding promo to order.',
        maxLength: 100,
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      RedemptionLimit: {
        type: 'integer',
        format: 'int32',
      },
      RedemptionLimitPerUser: {
        type: 'integer',
        format: 'int32',
      },
      RedemptionCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      FinePrint: {
        type: 'string',
        description: 'Terms, conditions, and other legal jargon.',
        maxLength: 2000,
      },
      StartDate: {
        type: 'string',
        format: 'date-time',
      },
      ExpirationDate: {
        type: 'string',
        format: 'date-time',
      },
      EligibleExpression: {
        type: 'string',
        maxLength: 400,
      },
      ValueExpression: {
        type: 'string',
        maxLength: 400,
      },
      CanCombine: {
        type: 'boolean',
      },
      AllowAllBuyers: {
        type: 'boolean',
        description:
          'Allow promo to be used by all buyers without creating assignments.',
      },
      xp: {
        type: 'object',
      },
    },
  },
  PasswordConfig: {
    type: 'object',
    example: {
      ExpireInDays: 0,
    },
    properties: {
      ExpireInDays: {
        type: 'integer',
        format: 'int32',
        minimum: 1,
      },
    },
  },
  PasswordReset: {
    type: 'object',
    example: {
      ClientID: '',
      Username: '',
      Password: '',
    },
    properties: {
      ClientID: {
        type: 'string',
      },
      Username: {
        type: 'string',
      },
      Password: {
        type: 'string',
        format: 'password',
      },
    },
  },
  PasswordResetRequest: {
    type: 'object',
    example: {
      ClientID: '',
      Email: '',
      Username: '',
      URL: '',
    },
    properties: {
      ClientID: {
        type: 'string',
      },
      Email: {
        type: 'string',
      },
      Username: {
        type: 'string',
      },
      URL: {
        type: 'string',
      },
    },
  },
  Payment: {
    type: 'object',
    example: {
      ID: '',
      Type: 'PurchaseOrder',
      DateCreated: '2018-01-01T00:00:00-06:00',
      CreditCardID: '',
      SpendingAccountID: '',
      Description: '',
      Amount: 0,
      Accepted: false,
      xp: {},
      Transactions: [
        {
          ID: '',
          Type: '',
          DateExecuted: '2018-01-01T00:00:00-06:00',
          Amount: 0,
          Succeeded: false,
          ResultCode: '',
          ResultMessage: '',
          xp: {},
        },
      ],
    },
    properties: {
      ID: {
        type: 'string',
      },
      Type: {
        enum: ['PurchaseOrder', 'CreditCard', 'SpendingAccount'],
        type: 'string',
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      CreditCardID: {
        type: 'string',
      },
      SpendingAccountID: {
        type: 'string',
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      Amount: {
        type: 'number',
        format: 'float',
        description:
          'If null, Payment applies to order total (or total of specific Line Items, if set), minus any other Payments where Amount is set.',
        minimum: 0.01,
      },
      Accepted: {
        type: 'boolean',
      },
      xp: {
        type: 'object',
      },
      Transactions: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/PaymentTransaction',
        },
        readOnly: true,
      },
    },
  },
  PaymentTransaction: {
    type: 'object',
    example: {
      ID: '',
      Type: '',
      DateExecuted: '2018-01-01T00:00:00-06:00',
      Amount: 0,
      Succeeded: false,
      ResultCode: '',
      ResultMessage: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Type: {
        type: 'string',
      },
      DateExecuted: {
        type: 'string',
        format: 'date-time',
      },
      Amount: {
        type: 'number',
        format: 'float',
        description:
          'Usually the same as Payment Amount, but can be different. A charge might have a subsequent partial credit, for example.',
      },
      Succeeded: {
        type: 'boolean',
      },
      ResultCode: {
        type: 'string',
      },
      ResultMessage: {
        type: 'string',
      },
      xp: {
        type: 'object',
      },
    },
  },
  PriceBreak: {
    type: 'object',
    example: {
      Quantity: 0,
      Price: 0,
    },
    properties: {
      Quantity: {
        type: 'integer',
        format: 'int32',
        minimum: 0,
      },
      Price: {
        type: 'number',
        format: 'float',
      },
    },
  },
  PriceSchedule: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      ApplyTax: false,
      ApplyShipping: false,
      MinQuantity: 0,
      MaxQuantity: 0,
      UseCumulativeQuantity: false,
      RestrictedQuantity: false,
      PriceBreaks: [
        {
          Quantity: 0,
          Price: 0,
        },
      ],
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      ApplyTax: {
        type: 'boolean',
      },
      ApplyShipping: {
        type: 'boolean',
      },
      MinQuantity: {
        type: 'integer',
        format: 'int32',
        default: 1,
        minimum: 1,
      },
      MaxQuantity: {
        type: 'integer',
        format: 'int32',
      },
      UseCumulativeQuantity: {
        type: 'boolean',
        description:
          'If true, LineItem quantities will be aggregated by productID when determining which price break applies. Else, each LineItem is treated separately.',
      },
      RestrictedQuantity: {
        type: 'boolean',
        description:
          'If true, this product can only be ordered in quantities that exactly match one of the price breaks on this schedule.',
      },
      PriceBreaks: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/PriceBreak',
        },
      },
      xp: {
        type: 'object',
      },
    },
  },
  Product: {
    type: 'object',
    example: {
      DefaultPriceScheduleID: '',
      ID: '',
      Name: '',
      Description: '',
      QuantityMultiplier: 0,
      ShipWeight: 0,
      ShipHeight: 0,
      ShipWidth: 0,
      ShipLength: 0,
      Active: false,
      SpecCount: 0,
      xp: {},
      VariantCount: 0,
      ShipFromAddressID: '',
      Inventory: {
        Enabled: false,
        NotificationPoint: 0,
        VariantLevelTracking: false,
        OrderCanExceed: false,
        QuantityAvailable: 0,
        LastUpdated: '2018-01-01T00:00:00-06:00',
      },
      DefaultSupplierID: '',
    },
    properties: {
      DefaultPriceScheduleID: {
        type: 'string',
      },
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      QuantityMultiplier: {
        type: 'integer',
        format: 'int32',
        default: 1,
        minimum: 1,
      },
      ShipWeight: {
        type: 'number',
        format: 'float',
      },
      ShipHeight: {
        type: 'number',
        format: 'float',
      },
      ShipWidth: {
        type: 'number',
        format: 'float',
      },
      ShipLength: {
        type: 'number',
        format: 'float',
      },
      Active: {
        type: 'boolean',
      },
      SpecCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      xp: {
        type: 'object',
      },
      VariantCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      ShipFromAddressID: {
        type: 'string',
      },
      Inventory: {
        allOf: [
          {
            $ref: '#/components/schemas/Inventory',
          },
        ],
      },
      DefaultSupplierID: {
        type: 'string',
      },
    },
  },
  ProductAssignment: {
    type: 'object',
    example: {
      ProductID: '',
      BuyerID: '',
      UserGroupID: '',
      PriceScheduleID: '',
    },
    properties: {
      ProductID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      PriceScheduleID: {
        type: 'string',
      },
    },
  },
  ProductCatalogAssignment: {
    type: 'object',
    example: {
      CatalogID: '',
      ProductID: '',
    },
    properties: {
      CatalogID: {
        type: 'string',
      },
      ProductID: {
        type: 'string',
      },
    },
  },
  ProductFacet: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      XpPath: '',
      ListOrder: 1,
      MinCount: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      XpPath: {
        type: 'string',
        description:
          'Optional. Identifies full path to xp field used for this facet. If not provided, facet value assumed to be stored at product.xp.{facet ID}.',
        maxLength: 200,
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
      },
      MinCount: {
        type: 'integer',
        format: 'int32',
        default: 1,
        description:
          'Minimum count required or a facet value to be returned in list metadata. Default is 1. If you want zero-count values returned, set this to 0.',
      },
      xp: {
        type: 'object',
      },
    },
  },
  Promotion: {
    type: 'object',
    example: {
      ID: '',
      Code: '',
      Name: '',
      RedemptionLimit: 0,
      RedemptionLimitPerUser: 0,
      RedemptionCount: 0,
      Description: '',
      FinePrint: '',
      StartDate: '2018-01-01T00:00:00-06:00',
      ExpirationDate: '2018-01-01T00:00:00-06:00',
      EligibleExpression: '',
      ValueExpression: '',
      CanCombine: false,
      AllowAllBuyers: false,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Code: {
        type: 'string',
        description:
          'Must be unique. Entered by buyer when adding promo to order.',
        maxLength: 100,
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      RedemptionLimit: {
        type: 'integer',
        format: 'int32',
      },
      RedemptionLimitPerUser: {
        type: 'integer',
        format: 'int32',
      },
      RedemptionCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      FinePrint: {
        type: 'string',
        description: 'Terms, conditions, and other legal jargon.',
        maxLength: 2000,
      },
      StartDate: {
        type: 'string',
        format: 'date-time',
      },
      ExpirationDate: {
        type: 'string',
        format: 'date-time',
      },
      EligibleExpression: {
        type: 'string',
        maxLength: 400,
      },
      ValueExpression: {
        type: 'string',
        maxLength: 400,
      },
      CanCombine: {
        type: 'boolean',
      },
      AllowAllBuyers: {
        type: 'boolean',
        description:
          'Allow promo to be used by all buyers without creating assignments.',
      },
      xp: {
        type: 'object',
      },
    },
  },
  PromotionAssignment: {
    type: 'object',
    example: {
      PromotionID: '',
      BuyerID: '',
      UserGroupID: '',
    },
    properties: {
      PromotionID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
    },
  },
  SecurityProfile: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Roles: ['DevCenter'],
      CustomRoles: [''],
      PasswordConfig: {
        ExpireInDays: 0,
      },
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Roles: {
        type: 'array',
        enum: [
          'DevCenter',
          'DevCenterPasswordReset',
          'DevCenterValidateEmail',
          'GrantForAnyRole',
          'ApiClientAdmin',
          'ApiClientReader',
          'AddressAdmin',
          'AddressReader',
          'AdminAddressAdmin',
          'AdminAddressReader',
          'AdminUserAdmin',
          'AdminUserGroupAdmin',
          'AdminUserGroupReader',
          'AdminUserReader',
          'ApprovalRuleAdmin',
          'ApprovalRuleReader',
          'BuyerAdmin',
          'BuyerImpersonation',
          'BuyerReader',
          'BuyerUserAdmin',
          'BuyerUserReader',
          'CatalogAdmin',
          'CatalogReader',
          'CategoryAdmin',
          'CategoryReader',
          'CostCenterAdmin',
          'CostCenterReader',
          'CreditCardAdmin',
          'CreditCardReader',
          'FullAccess',
          'IncrementorAdmin',
          'IncrementorReader',
          'InventoryAdmin',
          'MeAddressAdmin',
          'MeAdmin',
          'MeCreditCardAdmin',
          'MessageConfigAssignmentAdmin',
          'MeXpAdmin',
          'OrderAdmin',
          'OrderReader',
          'OverrideShipping',
          'OverrideTax',
          'OverrideUnitPrice',
          'PasswordReset',
          'PriceScheduleAdmin',
          'PriceScheduleReader',
          'ProductAdmin',
          'ProductAssignmentAdmin',
          'ProductFacetAdmin',
          'ProductFacetReader',
          'ProductReader',
          'PromotionAdmin',
          'PromotionReader',
          'SecurityProfileAdmin',
          'SecurityProfileReader',
          'SetSecurityProfile',
          'ShipmentAdmin',
          'ShipmentReader',
          'Shopper',
          'SpendingAccountAdmin',
          'SpendingAccountReader',
          'SupplierAddressAdmin',
          'SupplierAddressReader',
          'SupplierAdmin',
          'SupplierReader',
          'SupplierUserAdmin',
          'SupplierUserGroupAdmin',
          'SupplierUserGroupReader',
          'SupplierUserReader',
          'UnsubmittedOrderReader',
          'UserGroupAdmin',
          'UserGroupReader',
          'OpenIDConnectReader',
          'OpenIDConnectAdmin',
          'MessageSenderReader',
          'MessageSenderAdmin',
          'XpIndexAdmin',
          'WebhookReader',
          'WebhookAdmin',
        ],
        items: {
          type: 'string',
        },
      },
      CustomRoles: {
        type: 'array',
        items: {
          type: 'string',
        },
      },
      PasswordConfig: {
        allOf: [
          {
            $ref: '#/components/schemas/PasswordConfig',
          },
        ],
        format: 'password',
      },
    },
  },
  SecurityProfileAssignment: {
    type: 'object',
    example: {
      SecurityProfileID: '',
      BuyerID: '',
      SupplierID: '',
      UserID: '',
      UserGroupID: '',
    },
    properties: {
      SecurityProfileID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      SupplierID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
    },
  },
  Shipment: {
    type: 'object',
    example: {
      ID: '',
      BuyerID: '',
      Shipper: '',
      DateShipped: '2018-01-01T00:00:00-06:00',
      DateDelivered: '2018-01-01T00:00:00-06:00',
      TrackingNumber: '',
      Cost: 0,
      xp: {},
      Account: '',
      FromAddressID: '',
      ToAddressID: '',
      FromAddress: {
        ID: '',
        DateCreated: '2018-01-01T00:00:00-06:00',
        CompanyName: '',
        FirstName: '',
        LastName: '',
        Street1: '',
        Street2: '',
        City: '',
        State: '',
        Zip: '',
        Country: '',
        Phone: '',
        AddressName: '',
        xp: {},
      },
      ToAddress: {
        ID: '',
        DateCreated: '2018-01-01T00:00:00-06:00',
        CompanyName: '',
        FirstName: '',
        LastName: '',
        Street1: '',
        Street2: '',
        City: '',
        State: '',
        Zip: '',
        Country: '',
        Phone: '',
        AddressName: '',
        xp: {},
      },
    },
    properties: {
      ID: {
        type: 'string',
      },
      BuyerID: {
        type: 'string',
      },
      Shipper: {
        type: 'string',
      },
      DateShipped: {
        type: 'string',
        format: 'date-time',
      },
      DateDelivered: {
        type: 'string',
        format: 'date-time',
      },
      TrackingNumber: {
        type: 'string',
        maxLength: 3000,
      },
      Cost: {
        type: 'number',
        format: 'float',
      },
      xp: {
        type: 'object',
      },
      Account: {
        type: 'string',
      },
      FromAddressID: {
        type: 'string',
      },
      ToAddressID: {
        type: 'string',
      },
      FromAddress: {
        allOf: [
          {
            $ref: '#/components/schemas/Address',
          },
        ],
        readOnly: true,
      },
      ToAddress: {
        allOf: [
          {
            $ref: '#/components/schemas/Address',
          },
        ],
        readOnly: true,
      },
    },
  },
  ShipmentItem: {
    type: 'object',
    example: {
      OrderID: '',
      LineItemID: '',
      QuantityShipped: 0,
      UnitPrice: 0,
      CostCenter: '',
      DateNeeded: '2018-01-01T00:00:00-06:00',
      Product: {
        ID: '',
        Name: '',
        Description: '',
        QuantityMultiplier: 0,
        ShipWeight: 0,
        ShipHeight: 0,
        ShipWidth: 0,
        ShipLength: 0,
        xp: {},
      },
      Variant: {
        ID: '',
        Name: '',
        Description: '',
        ShipWeight: 0,
        ShipHeight: 0,
        ShipWidth: 0,
        ShipLength: 0,
        xp: {},
      },
      Specs: [
        {
          SpecID: '',
          Name: '',
          OptionID: '',
          Value: '',
        },
      ],
      xp: {},
    },
    properties: {
      OrderID: {
        type: 'string',
      },
      LineItemID: {
        type: 'string',
      },
      QuantityShipped: {
        type: 'integer',
        format: 'int32',
      },
      UnitPrice: {
        type: 'number',
        format: 'float',
        readOnly: true,
      },
      CostCenter: {
        type: 'string',
        readOnly: true,
      },
      DateNeeded: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      Product: {
        allOf: [
          {
            $ref: '#/components/schemas/LineItemProduct',
          },
        ],
        readOnly: true,
      },
      Variant: {
        allOf: [
          {
            $ref: '#/components/schemas/LineItemVariant',
          },
        ],
        readOnly: true,
      },
      Specs: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/LineItemSpec',
        },
        readOnly: true,
      },
      xp: {
        type: 'object',
        readOnly: true,
      },
    },
  },
  Spec: {
    type: 'object',
    example: {
      OptionCount: 0,
      ID: '',
      ListOrder: 1,
      Name: '',
      DefaultValue: '',
      Required: false,
      AllowOpenText: false,
      DefaultOptionID: '',
      DefinesVariant: false,
      xp: {},
    },
    properties: {
      OptionCount: {
        type: 'integer',
        format: 'int32',
        readOnly: true,
      },
      ID: {
        type: 'string',
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
      },
      Name: {
        type: 'string',
      },
      DefaultValue: {
        type: 'string',
        maxLength: 2000,
      },
      Required: {
        type: 'boolean',
      },
      AllowOpenText: {
        type: 'boolean',
      },
      DefaultOptionID: {
        type: 'string',
      },
      DefinesVariant: {
        type: 'boolean',
        description:
          "True if each unique combinations of this Spec's Options map to unique Product Variants/SKUs.",
      },
      xp: {
        type: 'object',
      },
    },
  },
  SpecOption: {
    type: 'object',
    example: {
      ID: '',
      Value: '',
      ListOrder: 1,
      IsOpenText: false,
      PriceMarkupType: 'NoMarkup',
      PriceMarkup: 0,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Value: {
        type: 'string',
        maxLength: 2000,
      },
      ListOrder: {
        type: 'integer',
        format: 'int32',
      },
      IsOpenText: {
        type: 'boolean',
      },
      PriceMarkupType: {
        enum: ['NoMarkup', 'AmountPerQuantity', 'AmountTotal', 'Percentage'],
        type: 'string',
      },
      PriceMarkup: {
        type: 'number',
        format: 'float',
      },
      xp: {
        type: 'object',
      },
    },
  },
  SpecProductAssignment: {
    type: 'object',
    example: {
      SpecID: '',
      ProductID: '',
      DefaultValue: '',
      DefaultOptionID: '',
    },
    properties: {
      SpecID: {
        type: 'string',
      },
      ProductID: {
        type: 'string',
      },
      DefaultValue: {
        type: 'string',
        description:
          'Optional. When defined, overrides the DefaultValue set on the Spec for just this Product.',
        maxLength: 2000,
      },
      DefaultOptionID: {
        type: 'string',
        description:
          'Optional. When defined, overrides the DefaultOptionID set on the Spec for just this Product.',
      },
    },
  },
  SpendingAccount: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Balance: 0,
      AllowAsPaymentMethod: false,
      RedemptionCode: '',
      StartDate: '2018-01-01T00:00:00-06:00',
      EndDate: '2018-01-01T00:00:00-06:00',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Balance: {
        type: 'number',
        format: 'float',
      },
      AllowAsPaymentMethod: {
        type: 'boolean',
      },
      RedemptionCode: {
        type: 'string',
        description:
          'If specified, matching code must be provided on redemption in order for the transaction to be successful. Most commonly used to implement Gift Cards.',
      },
      StartDate: {
        type: 'string',
        format: 'date-time',
      },
      EndDate: {
        type: 'string',
        format: 'date-time',
      },
      xp: {
        type: 'object',
      },
    },
  },
  SpendingAccountAssignment: {
    type: 'object',
    example: {
      SpendingAccountID: '',
      UserID: '',
      UserGroupID: '',
      AllowExceed: false,
    },
    properties: {
      SpendingAccountID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
      UserGroupID: {
        type: 'string',
      },
      AllowExceed: {
        type: 'boolean',
      },
    },
  },
  Supplier: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Active: false,
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
        maxLength: 100,
      },
      Active: {
        type: 'boolean',
      },
      xp: {
        type: 'object',
      },
    },
  },
  TokenPasswordReset: {
    type: 'object',
    example: {
      NewPassword: '',
    },
    properties: {
      NewPassword: {
        type: 'string',
        format: 'password',
      },
    },
  },
  User: {
    type: 'object',
    example: {
      ID: '',
      Username: '',
      Password: '',
      FirstName: '',
      LastName: '',
      Email: '',
      Phone: '',
      TermsAccepted: '2018-01-01T00:00:00-06:00',
      Active: false,
      xp: {},
      AvailableRoles: [''],
      DateCreated: '2018-01-01T00:00:00-06:00',
      PasswordLastSetDate: '2018-01-01T00:00:00-06:00',
    },
    properties: {
      ID: {
        type: 'string',
      },
      Username: {
        type: 'string',
        maxLength: 100,
      },
      Password: {
        type: 'string',
        format: 'password',
      },
      FirstName: {
        type: 'string',
        maxLength: 100,
      },
      LastName: {
        type: 'string',
        maxLength: 100,
      },
      Email: {
        type: 'string',
        maxLength: 200,
      },
      Phone: {
        type: 'string',
        maxLength: 100,
      },
      TermsAccepted: {
        type: 'string',
        format: 'date-time',
      },
      Active: {
        type: 'boolean',
      },
      xp: {
        type: 'object',
      },
      AvailableRoles: {
        type: 'array',
        items: {
          type: 'string',
        },
        readOnly: true,
      },
      DateCreated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
      PasswordLastSetDate: {
        type: 'string',
        format: 'password',
        readOnly: true,
      },
    },
  },
  UserGroup: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
        validators: [ValidateNoSpecialCharactersAndSpaces]
      },
      Name: {
        type: 'string',
        maxLength: 100,
        validators: [Validators.required]
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      xp: {
        type: 'object',
      },
    },
  },
  UserGroupAssignment: {
    type: 'object',
    example: {
      UserGroupID: '',
      UserID: '',
    },
    properties: {
      UserGroupID: {
        type: 'string',
      },
      UserID: {
        type: 'string',
      },
    },
  },
  Variant: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      Active: false,
      ShipWeight: 0,
      ShipHeight: 0,
      ShipWidth: 0,
      ShipLength: 0,
      Inventory: {
        QuantityAvailable: 0,
        LastUpdated: '2018-01-01T00:00:00-06:00',
      },
      xp: {},
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      Active: {
        type: 'boolean',
      },
      ShipWeight: {
        type: 'number',
        format: 'float',
      },
      ShipHeight: {
        type: 'number',
        format: 'float',
      },
      ShipWidth: {
        type: 'number',
        format: 'float',
      },
      ShipLength: {
        type: 'number',
        format: 'float',
      },
      Inventory: {
        allOf: [
          {
            $ref: '#/components/schemas/VariantInventory',
          },
        ],
      },
      xp: {
        type: 'object',
      },
    },
  },
  VariantInventory: {
    type: 'object',
    example: {
      QuantityAvailable: 0,
      LastUpdated: '2018-01-01T00:00:00-06:00',
    },
    properties: {
      QuantityAvailable: {
        type: 'integer',
        format: 'int32',
      },
      LastUpdated: {
        type: 'string',
        format: 'date-time',
        readOnly: true,
      },
    },
  },
  Webhook: {
    type: 'object',
    example: {
      ID: '',
      Name: '',
      Description: '',
      Url: '',
      HashKey: '',
      ElevatedRoles: ['DevCenter'],
      ConfigData: {},
      BeforeProcessRequest: false,
      ApiClientIDs: [''],
      WebhookRoutes: [
        {
          Route: '',
          Verb: '',
        },
      ],
    },
    properties: {
      ID: {
        type: 'string',
      },
      Name: {
        type: 'string',
      },
      Description: {
        type: 'string',
        maxLength: 2000,
      },
      Url: {
        type: 'string',
      },
      HashKey: {
        type: 'string',
      },
      ElevatedRoles: {
        type: 'array',
        enum: [
          'DevCenter',
          'DevCenterPasswordReset',
          'DevCenterValidateEmail',
          'GrantForAnyRole',
          'ApiClientAdmin',
          'ApiClientReader',
          'AddressAdmin',
          'AddressReader',
          'AdminAddressAdmin',
          'AdminAddressReader',
          'AdminUserAdmin',
          'AdminUserGroupAdmin',
          'AdminUserGroupReader',
          'AdminUserReader',
          'ApprovalRuleAdmin',
          'ApprovalRuleReader',
          'BuyerAdmin',
          'BuyerImpersonation',
          'BuyerReader',
          'BuyerUserAdmin',
          'BuyerUserReader',
          'CatalogAdmin',
          'CatalogReader',
          'CategoryAdmin',
          'CategoryReader',
          'CostCenterAdmin',
          'CostCenterReader',
          'CreditCardAdmin',
          'CreditCardReader',
          'FullAccess',
          'IncrementorAdmin',
          'IncrementorReader',
          'InventoryAdmin',
          'MeAddressAdmin',
          'MeAdmin',
          'MeCreditCardAdmin',
          'MessageConfigAssignmentAdmin',
          'MeXpAdmin',
          'OrderAdmin',
          'OrderReader',
          'OverrideShipping',
          'OverrideTax',
          'OverrideUnitPrice',
          'PasswordReset',
          'PriceScheduleAdmin',
          'PriceScheduleReader',
          'ProductAdmin',
          'ProductAssignmentAdmin',
          'ProductFacetAdmin',
          'ProductFacetReader',
          'ProductReader',
          'PromotionAdmin',
          'PromotionReader',
          'SecurityProfileAdmin',
          'SecurityProfileReader',
          'SetSecurityProfile',
          'ShipmentAdmin',
          'ShipmentReader',
          'Shopper',
          'SpendingAccountAdmin',
          'SpendingAccountReader',
          'SupplierAddressAdmin',
          'SupplierAddressReader',
          'SupplierAdmin',
          'SupplierReader',
          'SupplierUserAdmin',
          'SupplierUserGroupAdmin',
          'SupplierUserGroupReader',
          'SupplierUserReader',
          'UnsubmittedOrderReader',
          'UserGroupAdmin',
          'UserGroupReader',
          'OpenIDConnectReader',
          'OpenIDConnectAdmin',
          'MessageSenderReader',
          'MessageSenderAdmin',
          'XpIndexAdmin',
          'WebhookReader',
          'WebhookAdmin',
        ],
        items: {
          type: 'string',
        },
      },
      ConfigData: {
        type: 'object',
      },
      BeforeProcessRequest: {
        type: 'boolean',
      },
      ApiClientIDs: {
        type: 'array',
        items: {
          type: 'string',
        },
      },
      WebhookRoutes: {
        type: 'array',
        items: {
          $ref: '#/components/schemas/WebhookRoute',
        },
      },
    },
  },
  WebhookRoute: {
    type: 'object',
    example: {
      Route: '',
      Verb: '',
    },
    properties: {
      Route: {
        type: 'string',
      },
      Verb: {
        type: 'string',
      },
    },
  },
  XpIndex: {
    type: 'object',
    example: {
      ThingType: 'Product',
      Key: '',
    },
    properties: {
      ThingType: {
        enum: [
          'Product',
          'Variant',
          'Order',
          'LineItem',
          'Address',
          'CostCenter',
          'CreditCard',
          'Payment',
          'Spec',
          'SpecOption',
          'UserGroup',
          'Company',
          'Category',
          'PriceSchedule',
          'Shipment',
          'SpendingAccount',
          'User',
          'Promotion',
          'ApprovalRule',
          'Catalog',
          'ProductFacet',
          'MessageSender',
        ],
        type: 'string',
      },
      Key: {
        type: 'string',
      },
    },
  },
  Authentication: {
    type: 'object',
    properties: {
      access_token: {
        type: 'string',
      },
    },
  },
}

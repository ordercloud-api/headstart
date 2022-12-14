### Validating Setup

Once your marketplace has been seeded and your applications are configured you'll want to make sure everything is working well.

First, deploy the applications or run them locally:

- [Middleware](./src/Middleware/README.md)
- [Seller](./src/UI/Seller/README.md)
- [Buyer](./src/UI/Buyer/README.md)

The goal of this section is to show what steps are required to create a buyer experience on the buyer application, i.e. allowing a buyer user to browse and shop products and add them to their cart.

Overview of what we will accomplish

1. Create a Supplier and a Supplier User
2. Create a Buyer and a Buyer User
3. Create a Product and define product visibility to the User

> Note: You will have three logins by the end of this process - make sure to keep track of them:
>
> 1. Admin User Login
> 2. Supplier User Login
> 3. Buyer user Login

#### Step 1: Create Supplier(s)

On the OrderCloud platform, suppliers are an organization type used in indirect supply chain scenarios.

In the HeadStart application, suppliers are responsible for creating products so we will first need to create suppliers.

In the admin (seller) application:

1. Log in with your initial admin user credentials
2. From the navigation bar, click "Suppliers" > "All Suppliers"
3. Click "Create New Supplier"
   1. Fill in the required details and create the supplier
4. From the supplier details page, click "Supplier Addresses"
5. Click "Create New Supplier Address"
   1. Fill in the details and create a new address. **If AddressValidationProvider is enabled then use an actual address or it will fail address validation**
6. Navigate back to the supplier detail page via the breadcrumbs (i.e. Suppliers > _\<supplier id>_)
7. Click "Users" (*Note: There is already a user here with an ID that starts with `dev_` this user exists so that the middleware can act on behalf of it if needed to act as that supplier.* **Do not delete this user**)
8. Click "Create New User"
   1. Fill in the details
   2. Make sure "Active" is set to true
   3. Assign all permissions to the user
   4. If you have [emails set up](./src/Middleware/integrations/OrderCloud.Integrations.SendGrid/README.md) you can use the "forgot password" feature to set a password for the supplier user, otherwise set the password for that user in the portal

#### Step 2: Create Catalog(s)

In the admin (seller) application:

1. From the navigation bar, click on "Buyers" > "All Buyers"
2. Click the "Default HeadStart Buyer" buyer
3. Click "Catalogs"
4. Click "Create New Catalog" (*this will be the container that holds our products*)
    1. Enter a catalog name and click "Create"
5. Navigate back to the buyer detail page via the breadcrumbs (i.e. Buyers > _\<buyer id>_)
6. Click "Buyer Groups"
7. Click "Create New Buyer Group"
    1. Fill in the details. **If AddressValidationProvider is enabled then use an actual address or it will fail address validation**
    2. Under "Catalogs", set "assigned"to true for the catalog
8. Navigate back to the buyer detail page via the breadcrumbs (i.e. Buyers > _\<buyer id>_)
9. Click "Users"
10. Click "Create New User"
    1. Make sure "Active" is set to true
    2. Set the "Home Country" as the country defined for the buyer group address
    3. Under "Locations", set "assigned" to true for the buyer group
    4. If you have [emails set up](./src/Middleware/integrations/OrderCloud.Integrations.SendGrid/README.md) you can use the "forgot password" feature to set a password for the buyer user on your buyer application, otherwise set the password for that user in the portal

#### Step 3: Create Product(s)

At this point we've done all we can as a seller user. We now need to log in to the admin application as a supplier user to create our product.

In the admin (seller) application:

1. Log out of the initial admin user, if applicable
2. Log in as the supplier user from [Step 1: Create Supplier(s)](#step-1-create-suppliers)
3. Click on "Products" > "All Products"
4. Click "Create New Product" > "Standard Product"
    1. Enter the required fields (*required fields are marked with a red asterisk*)
    2. Make sure "Active" is set to true
    3. Click "Create" to save the product

Now that the product is created, our seller needs to define the visibility.

5. Log out of the supplier user
6. Log in as the initial admin user
7. Click on "Products" > "All Products"
8. Click on the previously created product
9. Click the "Buyer Visibility" tab
10. Click "Edit" on "Default Headstart Buyer"
    1. Set "visible" to true for the previously created catalog
    2. Click "Save"

#### Step 4: Review The Buyer App Experience

In the buyer application:

1. Log in as the buyer user from [Step 2: Create Catalog(s)](#step-2-create-catalogs)
2. Products can be viewed in the following ways:
    1. From the navigation bar, click "Products" (*returns all products*)
    2. From the search bar, type in a valid search term that will return products (*returns all products, filtered by search term*)
    3. Click the "SHOP" (category) navigation, then click a category from the category hierarchy. (*returns all products under the selected category*) (*Note: In [Step 2: Create Catalog(s)](#step-2-create-catalogs), no categories were created, therefore the category navigation menu may appear empty at this time*)
3. Add the product to the cart in the following ways:
    1. From the product details page: Click the product tile to navigate to the product details page, then click the "Add to Cart" button
    2. From the product results page: Click the "Add to Cart" button in the product tile of the product results page (*only applicable for products without variations*)
4. Products in the cart can be viewed in the following ways:
    1. Highlight the cart icon from the top menu, which will display the mini cart
    2. Click the "Edit Cart" link from the mini cart
    3. Click the cart icon from the top menu, which will display the cart page with additional functionality for managing the cart

Congrats! Hopefully you didn't get any errors and understand a little bit more about how everything is connected. If you did encounter errors please capture the details and submit an issue on Github.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
//using DynamicService.test_ms;
using DynamicService.usa_ms;
using Org.Softech.Inventory.Business.Objects;
using Org.Softech.Inventory.Business.Methods;
using Org.Softech.Utils;
using System.Transactions;
using Org.Softech.Exceptions.ExceptionHandling;
using Org.Softech.DataAccess.DataAccess;
using System.Data.Common;
using System.Data;
//using Org.Softech.Inventory.Utility;


namespace DynamicService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class DynamicService : IDynamicService
    {
        Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();

        usa_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient usaProxy = new usa_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();



        List<HFMagentoTransaction> magentoTransactions = new List<HFMagentoTransaction>();
        public void GetData(int value)
        {
            try
            {
                #region Soap

                Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();

                //string sessionId = proxy.login("admin", "admin123");
                string sessionId = proxy.login("softech", "admin123");

                #region Retrieve All Stores from magento & Save in Inventory

                storeEntity[] stores = proxy.storeList(sessionId);

                foreach (storeEntity item in stores)
                {
                    MagentoStore storeObj = new MagentoStore()
                    {
                        code = item.code,
                        group_id = item.group_id,
                        is_active = item.is_active,
                        name = item.name,
                        sort_order = item.sort_order,
                        store_id = item.store_id,
                        website_id = item.website_id
                    };

                    MagentoStoreBAL.SaveStore(storeObj);
                }

                #endregion

                #region Read Categories from magento & Save in Inventory

                catalogCategoryTree tree = proxy.catalogCategoryTree(sessionId, null, null);

                foreach (var item in tree.children)
                {
                    Category obj = new Category()
                    {
                        category_id = item.category_id,
                        is_active = item.is_active,
                        level = item.level,
                        name = item.name,
                        parent_id = item.parent_id,
                        position = item.position

                    };

                    CategoryBAL.SaveCategory(obj);
                }

                #endregion

                #region Read from magento & Save Products in Inventory
                catalogProductEntity[] products;
                proxy.catalogProductList(out products, sessionId, null, null);

                foreach (var item in products)
                {
                    Product obj = new Product()
                    {
                        name = item.name,
                        product_id = item.product_id,
                        set = item.set,
                        sku = item.sku,
                        type = item.type,
                        category_ids = item.category_ids,

                    };

                    ProductBAL.SaveProduct(obj);
                }
                #endregion

                #region Read Stock List

                //catalogProductEntity[] products;
                //proxy.catalogProductList(out products, sessionId, null, null);
                //string[] productIds = products.Select(s => s.product_id).ToArray();

                //catalogInventoryStockItemEntity[] stocks = proxy.catalogInventoryStockItemList(sessionId,productIds);   

                #endregion

                #region Stock Updation
                //catalogInventoryStockItemUpdateEntity obj = new catalogInventoryStockItemUpdateEntity()
                //{
                //    is_in_stock = 1,
                //    //qty = "53",
                //    //max_sale_qty = 20,
                //    //min_qty = 5,
                //    //min_sale_qty = 6,
                //    min_sale_qtySpecified = true,
                //    max_sale_qtySpecified = true,
                //    //use_config_max_sale_qty = 150,
                //    //use_config_min_qty = 8,
                //    //use_config_min_sale_qty = 9,
                //    //use_config_notify_stock_qty = 15,
                //    use_config_notify_stock_qtySpecified = false,
                //    notify_stock_qty = 22,
                //    notify_stock_qtySpecified = false,
                //    is_qty_decimal = 21,

                //};

                //proxy.catalogInventoryStockItemUpdate(sessionId, "1", obj);

                #endregion

                #region Create Category
                //catalogCategoryEntityCreate obj = new catalogCategoryEntityCreate()
                //{
                //    is_active = 1,
                //    name = "Inventory"
                //};
                //proxy.catalogCategoryCreate(sessionId, 0, obj, null); 
                //proxy.catalogProductCreate(
                #endregion


                #region Create Customer

                //customerCustomerEntityToCreate obj =
                // new customerCustomerEntityToCreate() 
                // {
                //     customer_id = 3,
                //     firstname = "Ahmed",
                //     lastname = "fayyaz",
                //     email = "ahmed.fayyaz@softech.com.pk",
                //     dob = "19/07/1972"
                // };

                // proxy.customerCustomerCreate(sessionId, obj );

                #endregion

                #region Retrieve Customers

                //customerCustomerEntity[] customers = proxy.customerCustomerList(sessionId, new filters());
                //foreach (customerCustomerEntity customer in customers)
                //{
                //    Console.WriteLine(String.Format("{0}. {1} {2}", customer.customer_id, customer.firstname, customer.lastname));
                //}

                #endregion

                proxy.endSession(sessionId);

                #endregion
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void InitializeProducts()
        {
            usa_ms.catalogProductEntity[] products = null;
            string sessionId = usaProxy.login("softech", "admin123");

            usaProxy.catalogProductList(out products, sessionId, null, null);

            foreach (var item in products)
            {
                if (Convert.ToInt32(item.product_id) > 76)
                {

                    usa_ms.catalogProductReturnEntity prod = usaProxy.catalogProductInfo(sessionId, item.product_id, null, null, item.product_id);
                    DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_PRODUCT_INFO");

                    //DataAccessHelper.CreateInParameter(command,"?p_category_ids", DbType.String, prod.            
                    DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, prod.created_at);
                    DataAccessHelper.CreateInParameter(command, "?p_custom_design", DbType.String, prod.custom_design);
                    DataAccessHelper.CreateInParameter(command, "?p_custom_layout_update", DbType.String, prod.custom_layout_update);
                    DataAccessHelper.CreateInParameter(command, "?p_description", DbType.String, prod.description);
                    DataAccessHelper.CreateInParameter(command, "?p_enable_googlecheckout", DbType.String, prod.enable_googlecheckout);
                    DataAccessHelper.CreateInParameter(command, "?p_gift_message_available", DbType.String, prod.gift_message_available);
                    DataAccessHelper.CreateInParameter(command, "?p_has_options", DbType.String, prod.has_options);
                    DataAccessHelper.CreateInParameter(command, "?p_meta_description", DbType.String, prod.meta_description);
                    DataAccessHelper.CreateInParameter(command, "?p_meta_keyword", DbType.String, prod.meta_keyword);
                    DataAccessHelper.CreateInParameter(command, "?p_meta_title", DbType.String, prod.meta_title);
                    DataAccessHelper.CreateInParameter(command, "?p_name", DbType.String, prod.name);
                    DataAccessHelper.CreateInParameter(command, "?p_options_container", DbType.String, prod.options_container);
                    DataAccessHelper.CreateInParameter(command, "?p_price", DbType.String, prod.price);
                    DataAccessHelper.CreateInParameter(command, "?p_product_id", DbType.String, prod.product_id);
                    DataAccessHelper.CreateInParameter(command, "?p_product_set", DbType.String, prod.set);
                    DataAccessHelper.CreateInParameter(command, "?p_short_description", DbType.String, prod.short_description);
                    DataAccessHelper.CreateInParameter(command, "?p_sku", DbType.String, prod.sku);
                    DataAccessHelper.CreateInParameter(command, "?p_special_from_date", DbType.String, prod.special_from_date);
                    DataAccessHelper.CreateInParameter(command, "?p_special_price", DbType.String, prod.special_price);
                    DataAccessHelper.CreateInParameter(command, "?p_special_to_date", DbType.String, prod.special_to_date);
                    DataAccessHelper.CreateInParameter(command, "?p_status", DbType.String, prod.status);
                    DataAccessHelper.CreateInParameter(command, "?p_tax_class_id", DbType.String, prod.tax_class_id);
                    DataAccessHelper.CreateInParameter(command, "?p_type", DbType.String, prod.type);
                    DataAccessHelper.CreateInParameter(command, "?p_type_id", DbType.String, prod.type_id);
                    DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, prod.updated_at);
                    DataAccessHelper.CreateInParameter(command, "?p_url_key", DbType.String, prod.url_key);
                    DataAccessHelper.CreateInParameter(command, "?p_url_path", DbType.String, prod.url_path);
                    DataAccessHelper.CreateInParameter(command, "?p_visibility", DbType.String, prod.visibility);
                    //DataAccessHelper.CreateInParameter(command,"?p_website_ids               
                    DataAccessHelper.CreateInParameter(command, "?p_weight", DbType.String, prod.weight);

                    bool data = DataAccessHelper.ExecuteNonQuery(command);
                }

            }

            usaProxy.endSession(sessionId);
        }

        public void InitializeCustomer()
        {
            usa_ms.customerCustomerEntity[] customers = null;
            string sessionId = usaProxy.login("softech", "admin123");

            customers = usaProxy.customerCustomerList(sessionId, null);

            foreach (var item in customers)
            {
                //if (item.customer_id > 170)
                //{
                    usa_ms.customerCustomerEntity cus = usaProxy.customerCustomerInfo(sessionId, item.customer_id, null);
                    DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_CUS_INFO");
                    DataAccessHelper.CreateInParameter(command, "?p_customer_id", DbType.Int32, cus.customer_id);
                    DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, cus.created_at);
                    DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, cus.updated_at);
                    DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, cus.increment_id);
                    DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.Int32, cus.store_id);
                    DataAccessHelper.CreateInParameter(command, "?p_website_id", DbType.Int32, cus.website_id);
                    DataAccessHelper.CreateInParameter(command, "?p_created_in", DbType.String, cus.created_in);
                    DataAccessHelper.CreateInParameter(command, "?p_email", DbType.String, cus.email);
                    DataAccessHelper.CreateInParameter(command, "?p_firstname", DbType.String, cus.firstname);
                    DataAccessHelper.CreateInParameter(command, "?p_middlename", DbType.String, cus.middlename);
                    DataAccessHelper.CreateInParameter(command, "?p_lastname", DbType.String, cus.lastname);
                    DataAccessHelper.CreateInParameter(command, "?p_group_id", DbType.Int32, cus.group_id);
                    DataAccessHelper.CreateInParameter(command, "?p_prefix", DbType.String, cus.prefix);
                    DataAccessHelper.CreateInParameter(command, "?p_suffix", DbType.String, cus.suffix);
                    DataAccessHelper.CreateInParameter(command, "?p_dob", DbType.String, cus.dob);
                    DataAccessHelper.CreateInParameter(command, "?p_taxvat", DbType.String, cus.taxvat);
                    DataAccessHelper.CreateInParameter(command, "?p_confirmation", DbType.Boolean, cus.confirmation);
                    DataAccessHelper.CreateInParameter(command, "?p_password_hash", DbType.String, cus.password_hash);
                    //DataAccessHelper.CreateInParameter(command, "?rp_token", DbType.String, cus.r);
                    //DataAccessHelper.CreateInParameter(command, "?rp_token_created_at", DbType.String, cus); 

                    bool data = DataAccessHelper.ExecuteNonQuery(command);
                //}

            }

            usaProxy.endSession(sessionId);
        
        }

        public void InitializeCustomerDetails()
        {
            usa_ms.customerCustomerEntity[] customers = null;
            string sessionId = usaProxy.login("softech", "admin123");

            List<int> failedIds = new List<int>();

            customers = usaProxy.customerCustomerList(sessionId, null);

            customers = customers.Where(a => a.customer_id > 0).Select(b => b).ToArray();

            Int64? org = 15706;
            InvSystem oInvsystem = new InvSystem();
            List<InvSystem> lInvSystem = new List<InvSystem>();
            Filters oFilter = new Filters();

            oFilter.AddParameters(() => oInvsystem.Organisation, OperatorsList.Equal, org);


            lInvSystem = InvSystemBAL.LoadSystems(oInvsystem, oFilter);

            List<int> addedcustomers = new List<int>();

            foreach (var item in customers)
            {
                
                int added = addedcustomers.Where(a => a == item.customer_id).Count();
                if (added > 0)
                {
                    continue;
                    // No point of re adding an already added customer... 
                }
                else
                {
                    try
                    {
                //        usa_ms.customerCustomerEntity mcus = usaProxy.customerCustomerInfo(sessionId, item.customer_id, null);
                        usa_ms.customerCustomerEntity mcus = item;

                        usa_ms.customerAddressEntityItem[] addr = usaProxy.customerAddressList(sessionId, item.customer_id);

                        Customer cus = new Customer()
                        {
                            CustomerName = mcus.firstname + " " + mcus.lastname,
                            Email = mcus.email,
                            City = addr.Count() > 0 ? addr[0].city : "",
                            BillingCity = addr.Count() > 0 ? addr[0].city : "",
                            BillingAddress = addr.Count() > 0 ? addr[0].street + " " + addr[0].region : "",
                            ZipCode = addr.Count() > 0 ? addr[0].postcode : "",
                            BillingZip = addr.Count() > 0 ? addr[0].postcode : "",
                            CellNo = addr.Count() > 0 ? addr[0].telephone : "",
                            Country = addr.Count() > 0 ? addr[0].country_id : "",
                            BillingState = addr.Count() > 0 ? addr[0].region : "",
                            BillingCountry = addr.Count() > 0 ? addr[0].country_id : "",
                            ParentCode = lInvSystem[0].DefaultDistributor,
                            MagentoCustomerId = item.customer_id.ToString(),
                            CustomerLevel = 3,
                            Organization = 15706
                        };

                        cus = CustomerBAL.SetCustomerCode(cus);

                        List<Customer> invcustomers = CustomerBAL.LoadCustomer(new Customer(), oFilter);

                        invcustomers = invcustomers.Where(a => a.MagentoCustomerId == item.customer_id.ToString()).Select(b => b).ToList();

                        bool result;

                        if (invcustomers.Count > 0)
                        {
                            cus.CustomerCode = invcustomers[0].CustomerCode;
                            result = CustomerBAL.UpdateCustomer(cus);
                        }
                        else
                        {
                            result = CustomerBAL.saveCustomer(cus);
                        }
                        if (!result)
                        {
                            failedIds.Add(item.customer_id);
                        }
                        else
                        {
                            addedcustomers.Add(item.customer_id);
                        }
                    }
                    catch (Exception)
                    {
                        failedIds.Add(item.customer_id);
                        continue;
                    }
                }

            }

            usaProxy.endSession(sessionId);

        }

        public void InitializeSalesOrderList()
        {
            usa_ms.salesOrderListEntity[] salesOrders = null;
            string sessionId = usaProxy.login("softech", "admin123");
            salesOrders = usaProxy.salesOrderList(sessionId, null);

            foreach (var sale in salesOrders)
            {

                //if (Convert.ToInt32(sale.increment_id) > 100000221)
                //{
                    DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORDER_LIST");
                    DataAccessHelper.CreateInParameter(command, "?p_gift_message_id", DbType.String, sale.gift_message_id);
                    DataAccessHelper.CreateInParameter(command, "?p_email_sent", DbType.String, sale.email_sent);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_is_guest", DbType.String, sale.customer_is_guest);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_note_notify", DbType.String, sale.customer_note_notify);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_group_id", DbType.String, sale.customer_group_id);
                    DataAccessHelper.CreateInParameter(command, "?p_is_virtual", DbType.String, sale.is_virtual);
                    DataAccessHelper.CreateInParameter(command, "?p_quote_id", DbType.String, sale.quote_id);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_lastname", DbType.String, sale.customer_lastname);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_firstname", DbType.String, sale.customer_firstname);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_method", DbType.String, sale.shipping_method);
                    DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, sale.order_currency_code);
                    DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, sale.store_currency_code);
                    DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, sale.base_currency_code);
                    DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, sale.global_currency_code);
                    DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, sale.state);
                    DataAccessHelper.CreateInParameter(command, "?p_status", DbType.String, sale.status);
                    DataAccessHelper.CreateInParameter(command, "?p_remote_ip", DbType.String, sale.remote_ip);
                    DataAccessHelper.CreateInParameter(command, "?p_store_name", DbType.String, sale.store_name);
                    DataAccessHelper.CreateInParameter(command, "?p_weight", DbType.String, sale.weight);
                    DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, sale.base_to_order_rate);
                    DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, sale.base_to_global_rate);
                    DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, sale.store_to_order_rate);
                    DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, sale.store_to_base_rate);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_name", DbType.String, sale.shipping_name);
                    DataAccessHelper.CreateInParameter(command, "?p_billing_name", DbType.String, sale.billing_name);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_lastname", DbType.String, sale.shipping_lastname);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_firstname", DbType.String, sale.shipping_firstname);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_address_id", DbType.String, sale.shipping_address_id);
                    DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, sale.billing_lastname);
                    DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, sale.billing_firstname);
                    DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, sale.billing_address_id);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_online_refunded", DbType.String, sale.base_total_online_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_invoiced", DbType.String, sale.base_total_invoiced);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_canceled", DbType.String, sale.base_total_canceled);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_qty_ordered", DbType.String, sale.base_total_qty_ordered);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_refunded", DbType.String, sale.base_total_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_base_total_paid", DbType.String, sale.base_total_paid);
                    DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, sale.base_grand_total);
                    DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, sale.base_subtotal);
                    DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, sale.base_discount_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, sale.base_shipping_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, sale.base_tax_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_total_online_refunded", DbType.String, sale.total_online_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_total_invoiced", DbType.String, sale.total_invoiced);
                    DataAccessHelper.CreateInParameter(command, "?p_total_canceled", DbType.String, sale.total_canceled);
                    DataAccessHelper.CreateInParameter(command, "?p_total_qty_ordered", DbType.String, sale.total_qty_ordered);
                    DataAccessHelper.CreateInParameter(command, "?p_total_refunded", DbType.String, sale.total_refunded);
                    DataAccessHelper.CreateInParameter(command, "?p_total_paid", DbType.String, sale.total_paid);
                    DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, sale.grand_total);
                    DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, sale.subtotal);
                    DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, sale.discount_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, sale.shipping_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, sale.tax_amount);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_id", DbType.String, sale.customer_id);
                    DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, sale.updated_at);
                    DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, sale.created_at);
                    DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, sale.store_id);
                    DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, sale.relation_parent_id); // parent id    
                    DataAccessHelper.CreateInParameter(command, "?p_shipping_description", DbType.String, sale.shipping_description);
                    DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, sale.order_id);
                    DataAccessHelper.CreateInParameter(command, "?p_customer_email", DbType.String, sale.customer_email);
                    DataAccessHelper.CreateInParameter(command, "?p_applied_rule_ids", DbType.String, sale.applied_rule_ids);
                    DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, sale.increment_id);
                    bool data = DataAccessHelper.ExecuteNonQuery(command);


                    usa_ms.filters fil = new usa_ms.filters();
                    usa_ms.associativeEntity ass = new usa_ms.associativeEntity() { key = "increment_id", value = sale.increment_id };
                    fil.filter = new usa_ms.associativeEntity[] { ass };

                    usa_ms.salesOrderInvoiceEntity[] invoices = usaProxy.salesOrderInvoiceList(sessionId, fil);
                    usa_ms.salesOrderInvoiceEntity entity = null;

                    if (invoices != null && invoices.Count() > 0)
                    {
                        entity = usaProxy.salesOrderInvoiceInfo(sessionId, invoices[0].increment_id);

                        SaveInvoiceDetail(entity);
                    }
                //}

            }


            usaProxy.endSession(sessionId);

        }

        private static void SaveInvoiceDetail(usa_ms.salesOrderInvoiceEntity entity)
        {
            DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORD_INV_INFO");

            DataAccessHelper.CreateInParameter(command, "?p_invoice_id", DbType.String, entity.invoice_id);
            DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, entity.grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, entity.state);
            DataAccessHelper.CreateInParameter(command, "?p_order_created_at", DbType.String, entity.order_created_at);
            DataAccessHelper.CreateInParameter(command, "?p_order_increment_id", DbType.String, entity.order_increment_id);
            DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, entity.order_id);
            DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, entity.billing_lastname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, entity.billing_firstname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, entity.billing_address_id);
            DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, entity.base_tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, entity.tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, entity.base_shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, entity.shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, entity.base_discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, entity.discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, entity.base_grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, entity.base_subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, entity.subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, entity.base_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, entity.base_to_global_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, entity.store_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, entity.store_to_base_rate);
            DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, entity.order_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, entity.store_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, entity.base_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, entity.global_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_is_active", DbType.String, entity.is_active);
            DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, entity.updated_at);
            DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, entity.created_at);
            DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, entity.store_id);
            DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, entity.parent_id);
            DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, entity.increment_id);
            //DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
            //DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

            bool data = DataAccessHelper.ExecuteNonQuery(command);


            foreach (usa_ms.salesOrderInvoiceItemEntity item in entity.items)
            {
                DbCommand command2 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORD_INV_ITEM");

                DataAccessHelper.CreateInParameter(command2, "p_item_id", DbType.String, item.item_id);
                DataAccessHelper.CreateInParameter(command2, "p_product_id", DbType.String, item.product_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_item_id", DbType.String, item.order_item_id);
                DataAccessHelper.CreateInParameter(command2, "p_name", DbType.String, item.name);
                DataAccessHelper.CreateInParameter(command2, "p_sku", DbType.String, item.sku);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_row_disposition", DbType.String, item.base_weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_disposition", DbType.String, item.base_weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_row_disposition", DbType.String, item.weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_disposition", DbType.String, item.weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_row_amount", DbType.String, item.weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_amount", DbType.String, item.weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_row_amount", DbType.String, item.base_weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_amount", DbType.String, item.base_weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_row_total", DbType.String, item.base_row_total);
                DataAccessHelper.CreateInParameter(command2, "p_base_tax_amount", DbType.String, item.base_tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_price", DbType.String, item.base_price);
                DataAccessHelper.CreateInParameter(command2, "p_row_total", DbType.String, item.row_total);
                DataAccessHelper.CreateInParameter(command2, "p_tax_amount", DbType.String, item.tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_price", DbType.String, item.price);
                DataAccessHelper.CreateInParameter(command2, "p_cost", DbType.String, item.cost);
                DataAccessHelper.CreateInParameter(command2, "p_qty", DbType.String, item.qty);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied", DbType.String, item.weee_tax_applied);
                DataAccessHelper.CreateInParameter(command2, "p_is_active", DbType.String, item.is_active);
                DataAccessHelper.CreateInParameter(command2, "p_updated_at", DbType.String, item.updated_at);
                DataAccessHelper.CreateInParameter(command2, "p_created_at", DbType.String, item.created_at);
                DataAccessHelper.CreateInParameter(command2, "p_parent_id", DbType.String, item.parent_id);
                DataAccessHelper.CreateInParameter(command2, "p_increment_id", DbType.String, item.increment_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_id", DbType.String, entity.order_id);
                //DataAccessHelper.CreateInParameter(command2, "p_softech_store_id", DbType.Int64, 1);
                //DataAccessHelper.CreateInParameter(command2, "?p_procedure_date", DbType.DateTime, DateTime.Now);


                bool data2 = DataAccessHelper.ExecuteNonQuery(command2);
            }


            foreach (usa_ms.salesOrderInvoiceCommentEntity item in entity.comments)
            {
                DbCommand command3 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORD_INV_COM_DETL");

                DataAccessHelper.CreateInParameter(command3, "p_comment_id", DbType.String, item.comment_id);
                DataAccessHelper.CreateInParameter(command3, "p_is_customer_notified", DbType.String, item.is_customer_notified);
                DataAccessHelper.CreateInParameter(command3, "p_is_active", DbType.String, item.is_active);
                DataAccessHelper.CreateInParameter(command3, "p_updated_at", DbType.String, item.updated_at);
                DataAccessHelper.CreateInParameter(command3, "p_created_at", DbType.String, item.created_at);
                DataAccessHelper.CreateInParameter(command3, "p_parent_id", DbType.String, item.parent_id);
                DataAccessHelper.CreateInParameter(command3, "p_comment", DbType.String, item.comment);
                DataAccessHelper.CreateInParameter(command3, "p_increment_id", DbType.String, item.increment_id);
                //DataAccessHelper.CreateInParameter(command3, "p_softech_store_id", DbType.Int64, 1);
                //DataAccessHelper.CreateInParameter(command3, "?p_procedure_date", DbType.DateTime, DateTime.Now);



                bool data3 = DataAccessHelper.ExecuteNonQuery(command3);
            }
        }

        public catalogProductEntity[] GetProducts(Int64 storeId)
        {
            try
            {
                catalogProductEntity[] products = null;
                //List<catalogProductReturnEntity> prods = new List<catalogProductReturnEntity>();
                if (storeId == 1) // For each store site
                {
                    Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    string sessionId = proxy.login("softech", "admin123");
                    #region Read from magento & Save Products in Inventory
                    proxy.catalogProductList(out products, sessionId, null, null);

                    //foreach (var item in products)
                    //{
                    //    if (Convert.ToInt64(item.product_id) != 77 && Convert.ToInt64(item.product_id) != 91)
                    //    {
                    //        catalogProductReturnEntity prod = usaProxy.catalogProductInfo(sessionId, item.product_id, null, null, item.product_id);
                    //        prods.Add(prod);
                    //    }
                    //}
                    


                    //foreach (var item in products)
                    //{
                    //    Product obj = new Product()
                    //    {
                    //        name = item.name,
                    //        product_id = item.product_id,
                    //        set = item.set,
                    //        sku = item.sku,
                    //        type = item.type,
                    //        category_ids = item.category_ids,

                    //    };

                    //    ProductBAL.SaveProduct(obj);
                    //}




                    proxy.endSession(sessionId);
                    #endregion
                }
                else if (storeId == 2)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }
                else if (storeId == 3)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }
                else if (storeId == 4)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }
                else if (storeId == 5)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }
                else if (storeId == 6)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }
                else if (storeId == 7)
                {
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                    //#region Read from magento & Save Products in Inventory
                    //proxy.catalogProductList(out products, sessionId, null, null);
                    //proxy.endSession(sessionId);
                    //#endregion
                }

                return products /*prods*/;

            }
            catch (Exception ex)
            {
                return null;
                //throw ex;
            }

        }

        public int CreateProduct(string sku, string desc, string shortDesc, string price, string weight,string p_qty)
        {
            int prodID = 0;
            try
            {

                string sessionId = BeginSession();

                catalogProductCreateEntity prod = new catalogProductCreateEntity()
                {
                    name = desc,
                    description = desc,
                    short_description = shortDesc,
                    weight = weight,
                    status = "1",
                    visibility = "4",
                    price = price,
                    tax_class_id = "1",
                     
                    stock_data = new catalogInventoryStockItemUpdateEntity() 
                    { 
                        qty = p_qty,
                        is_in_stock = 1,
                        is_in_stockSpecified = true ,
                        manage_stock = 1,
                       // manage_stockSpecified = false,
                        use_config_manage_stock = 0,
                       // use_config_manage_stockSpecified = false
                    }
                };


                prodID = proxy.catalogProductCreate(sessionId, "simple", "4", sku, prod, null);
                EndSession(sessionId);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return prodID;
        }

        public bool UpdateProduct(string productId, string price, string weight)
        {
            bool success = false;
            try
            {

                string sessionId = BeginSession();

                catalogProductCreateEntity prod = new catalogProductCreateEntity()
                {
                    //name = desc,
                    //description = desc,
                    //short_description = shortDesc,
                    weight = weight,
                    //status = "1",
                    //visibility = "4",
                    price = price,
                    //tax_class_id = "1",

                    //stock_data = new catalogInventoryStockItemUpdateEntity()
                    //{
                    //    qty = p_qty,
                    //    is_in_stock = 1,
                    //    is_in_stockSpecified = true,
                    //    manage_stock = 1,
                    //    // manage_stockSpecified = false,
                    //    use_config_manage_stock = 0,
                    //    // use_config_manage_stockSpecified = false
                    //}
                };


                proxy.catalogProductUpdate(sessionId, productId, prod, null , productId );

                EndSession(sessionId);

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return success;
        }


        public customerCustomerEntity[] GetCustomers()
        {

            string sessionId = BeginSession();
            customerCustomerEntity[] customers = null;
            //test_ms.filters fil = new filters();
            //test_ms.associativeEntity ass = new associativeEntity() { key = "status", value = "complete" };
            //fil.filter = new associativeEntity[] { ass };

            customers = proxy.customerCustomerList(sessionId, null);
            EndSession(sessionId);

            return customers;
        }



        #region For Getting customers from magento and storing in inventory
        public void SaveCustomers()
        {
            try
            {

                var customers = GetCustomers();
                foreach (customerCustomerEntity cus in customers)
                {
                    CustomerProfile prof = new CustomerProfile()
                    {
                        confirmationField = cus.confirmation,
                        created_atField = cus.created_at,
                        created_inField = cus.created_in,
                        customer_idField = cus.customer_id,
                        dobField = cus.dob,
                        emailField = cus.email,
                        firstnameField = cus.firstname,
                        group_idField = cus.group_id,
                        increment_idField = cus.increment_id,
                        lastnameField = cus.lastname,
                        middlenameField = cus.middlename,
                        password_hashField = cus.password_hash,
                        prefixField = cus.prefix,
                        store_idField = cus.store_id,
                        suffixField = cus.suffix,
                        taxvatField = cus.taxvat,
                        updated_atField = cus.updated_at,
                        website_idField = cus.website_id

                    };



                    MagentoOrderBAL.SaveMagentoCustomer(prof);

                }


            }
            catch (Exception ex)
            {
                bool rethrow = false;
                rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
            }
        }

        #endregion

        public salesOrderListEntity[] SaveSalesOrder(string sessionId,string order_id)
        {
            salesOrderListEntity[] salesOrders = null;
            try
            {

                //var salesOrders = GetSalesOrders();
                salesOrders = GetSalesOrdersByOrderID(sessionId,order_id);
                SaveSaleOrder(salesOrders);

                return salesOrders;


            }
            catch (Exception ex)
            {
                bool rethrow = false;
                rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                return salesOrders;
                if (rethrow)
                {
                    throw ex;
                }
            }
        }

        private static void SaveSaleOrder(salesOrderListEntity[] salesOrders)
        {
            foreach (salesOrderListEntity sale in salesOrders)
            {
                DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORDER");
                // DataAccessHelper.CreateInParameter(command, "?gift_message", DbType.String, sale.gift_message);   
                DataAccessHelper.CreateInParameter(command, "?p_gift_message_id", DbType.String, sale.gift_message_id);
                DataAccessHelper.CreateInParameter(command, "?p_email_sent", DbType.String, sale.email_sent);
                DataAccessHelper.CreateInParameter(command, "?p_customer_is_guest", DbType.String, sale.customer_is_guest);
                DataAccessHelper.CreateInParameter(command, "?p_customer_note_notify", DbType.String, sale.customer_note_notify);
                DataAccessHelper.CreateInParameter(command, "?p_customer_group_id", DbType.String, sale.customer_group_id);
                DataAccessHelper.CreateInParameter(command, "?p_is_virtual", DbType.String, sale.is_virtual);
                DataAccessHelper.CreateInParameter(command, "?p_quote_id", DbType.String, sale.quote_id);
                DataAccessHelper.CreateInParameter(command, "?p_customer_lastname", DbType.String, sale.customer_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_customer_firstname", DbType.String, sale.customer_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_method", DbType.String, sale.shipping_method);
                DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, sale.order_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, sale.store_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, sale.base_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, sale.global_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, sale.state);
                DataAccessHelper.CreateInParameter(command, "?p_status", DbType.String, sale.status);
                DataAccessHelper.CreateInParameter(command, "?p_remote_ip", DbType.String, sale.remote_ip);
                DataAccessHelper.CreateInParameter(command, "?p_store_name", DbType.String, sale.store_name);
                DataAccessHelper.CreateInParameter(command, "?p_weight", DbType.String, sale.weight);
                DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, sale.base_to_order_rate);
                DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, sale.base_to_global_rate);
                DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, sale.store_to_order_rate);
                DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, sale.store_to_base_rate);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_name", DbType.String, sale.shipping_name);
                DataAccessHelper.CreateInParameter(command, "?p_billing_name", DbType.String, sale.billing_name);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_lastname", DbType.String, sale.shipping_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_firstname", DbType.String, sale.shipping_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_address_id", DbType.String, sale.shipping_address_id);
                DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, sale.billing_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, sale.billing_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, sale.billing_address_id);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_online_refunded", DbType.String, sale.base_total_online_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_invoiced", DbType.String, sale.base_total_invoiced);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_canceled", DbType.String, sale.base_total_canceled);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_qty_ordered", DbType.String, sale.base_total_qty_ordered);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_refunded", DbType.String, sale.base_total_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_paid", DbType.String, sale.base_total_paid);
                DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, sale.base_grand_total);
                DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, sale.base_subtotal);
                DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, sale.base_discount_amount);
                DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, sale.base_shipping_amount);
                DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, sale.base_tax_amount);
                DataAccessHelper.CreateInParameter(command, "?p_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_online_refunded", DbType.String, sale.total_online_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_invoiced", DbType.String, sale.total_invoiced);
                DataAccessHelper.CreateInParameter(command, "?p_total_canceled", DbType.String, sale.total_canceled);
                DataAccessHelper.CreateInParameter(command, "?p_total_qty_ordered", DbType.String, sale.total_qty_ordered);
                DataAccessHelper.CreateInParameter(command, "?p_total_refunded", DbType.String, sale.total_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_paid", DbType.String, sale.total_paid);
                DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, sale.grand_total);
                DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, sale.subtotal);
                DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, sale.discount_amount);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, sale.shipping_amount);
                DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, sale.tax_amount);
                DataAccessHelper.CreateInParameter(command, "?p_customer_id", DbType.String, sale.customer_id);
                //DataAccessHelper.CreateInParameter(command,"?p_is_active"                  ,DbType.String, sale.is_active);                        
                DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, sale.updated_at);
                DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, sale.created_at);
                DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, sale.store_id);
                DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, sale.relation_parent_id); // parent id    
                DataAccessHelper.CreateInParameter(command, "?p_shipping_description", DbType.String, sale.shipping_description);
                DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, sale.order_id);
                DataAccessHelper.CreateInParameter(command, "?p_customer_email", DbType.String, sale.customer_email);
                DataAccessHelper.CreateInParameter(command, "?p_applied_rule_ids", DbType.String, sale.applied_rule_ids);
                DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, sale.increment_id);
                DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
                DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

                bool data = DataAccessHelper.ExecuteNonQuery(command);
                // return data;
            }
        }

        private static void SaveSaleOrder(MagentoOrder sale)
        {
            
                DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_ORDER");
                // DataAccessHelper.CreateInParameter(command, "?gift_message", DbType.String, sale.gift_message);   
                DataAccessHelper.CreateInParameter(command, "?p_gift_message_id", DbType.String, sale.gift_message_id);
                DataAccessHelper.CreateInParameter(command, "?p_email_sent", DbType.String, sale.email_sent);
                DataAccessHelper.CreateInParameter(command, "?p_customer_is_guest", DbType.String, sale.customer_is_guest);
                DataAccessHelper.CreateInParameter(command, "?p_customer_note_notify", DbType.String, sale.customer_note_notify);
                DataAccessHelper.CreateInParameter(command, "?p_customer_group_id", DbType.String, sale.customer_group_id);
                DataAccessHelper.CreateInParameter(command, "?p_is_virtual", DbType.String, sale.is_virtual);
                DataAccessHelper.CreateInParameter(command, "?p_quote_id", DbType.String, sale.quote_id);
                DataAccessHelper.CreateInParameter(command, "?p_customer_lastname", DbType.String, sale.customer_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_customer_firstname", DbType.String, sale.customer_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_method", DbType.String, sale.shipping_method);
                DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, sale.order_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, sale.store_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, sale.base_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, sale.global_currency_code);
                DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, sale.state);
                DataAccessHelper.CreateInParameter(command, "?p_status", DbType.String, sale.status);
                DataAccessHelper.CreateInParameter(command, "?p_remote_ip", DbType.String, sale.remote_ip);
                DataAccessHelper.CreateInParameter(command, "?p_store_name", DbType.String, sale.store_name);
                DataAccessHelper.CreateInParameter(command, "?p_weight", DbType.String, sale.weight);
                DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, sale.base_to_order_rate);
                DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, sale.base_to_global_rate);
                DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, sale.store_to_order_rate);
                DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, sale.store_to_base_rate);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_name", DbType.String, sale.shipping_name);
                DataAccessHelper.CreateInParameter(command, "?p_billing_name", DbType.String, sale.billing_name);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_lastname", DbType.String, sale.shipping_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_firstname", DbType.String, sale.shipping_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_address_id", DbType.String, sale.shipping_address_id);
                DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, sale.billing_lastname);
                DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, sale.billing_firstname);
                DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, sale.billing_address_id);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_online_refunded", DbType.String, sale.base_total_online_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_invoiced", DbType.String, sale.base_total_invoiced);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_canceled", DbType.String, sale.base_total_canceled);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_qty_ordered", DbType.String, sale.base_total_qty_ordered);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_refunded", DbType.String, sale.base_total_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_base_total_paid", DbType.String, sale.base_total_paid);
                DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, sale.base_grand_total);
                DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, sale.base_subtotal);
                DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, sale.base_discount_amount);
                DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, sale.base_shipping_amount);
                DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, sale.base_tax_amount);
                DataAccessHelper.CreateInParameter(command, "?p_total_offline_refunded", DbType.String, sale.base_total_offline_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_online_refunded", DbType.String, sale.total_online_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_invoiced", DbType.String, sale.total_invoiced);
                DataAccessHelper.CreateInParameter(command, "?p_total_canceled", DbType.String, sale.total_canceled);
                DataAccessHelper.CreateInParameter(command, "?p_total_qty_ordered", DbType.String, sale.total_qty_ordered);
                DataAccessHelper.CreateInParameter(command, "?p_total_refunded", DbType.String, sale.total_refunded);
                DataAccessHelper.CreateInParameter(command, "?p_total_paid", DbType.String, sale.total_paid);
                DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, sale.grand_total);
                DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, sale.subtotal);
                DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, sale.discount_amount);
                DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, sale.shipping_amount);
                DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, sale.tax_amount);
                DataAccessHelper.CreateInParameter(command, "?p_customer_id", DbType.String, sale.customer_id);
                //DataAccessHelper.CreateInParameter(command,"?p_is_active"                  ,DbType.String, sale.is_active);                        
                DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, sale.updated_at);
                DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, sale.created_at);
                DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, sale.store_id);
                DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, sale.relation_parent_id); // parent id    
                DataAccessHelper.CreateInParameter(command, "?p_shipping_description", DbType.String, sale.shipping_description);
                DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, sale.order_id);
                DataAccessHelper.CreateInParameter(command, "?p_customer_email", DbType.String, sale.customer_email);
                DataAccessHelper.CreateInParameter(command, "?p_applied_rule_ids", DbType.String, sale.applied_rule_ids);
                DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, sale.increment_id);
                DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
                DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

                bool data = DataAccessHelper.ExecuteNonQuery(command);
                // return data;
            
        }

        public List<salesOrderListEntity> GetSalesOrders(DateTime? orderDate, string orderID)
        {

            string sessionId = BeginSession();
             salesOrderListEntity[] salesOrders = null;
             filters fil = new filters();
             associativeEntity ass = new associativeEntity() { key = "status", value = "complete" };
            fil.filter = new associativeEntity[] { ass };

            #region Other way around
            /*var cpf = new complexFilter[2];
            cpf[0] = new complexFilter
            {
                key = "created_at",
                value = new associativeEntity
                {
                    key = "from",
                    value = orderDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };
            cpf[1] = new complexFilter
            {
                key = "created_at",
                value = new associativeEntity
                {
                    key = "to",
                    value = orderDate.Value.ToString("yyyy-MM-dd HH:mm:ss")
                }
            };            
            fil.complex_filter = cpf; */
            #endregion

            if (!String.IsNullOrEmpty(orderID))
            {
                 associativeEntity ass2 = new associativeEntity() { key = "increment_id", value = orderID };
                fil.filter = new associativeEntity[] { ass2 };
            }
            else
            {
                #region New way around
                               

                //const string createdAt = "created_at";
                //string magentoDateTimeFormat = "yyyy-MM-dd";
                //magentoDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
                //DateTime toDate = DateTime.Now;
                //DateTime fromDate =  DateTime.Now.AddDays(-1);

                //complexFilter[] cpf = new complexFilter[3];
                //cpf[0] = new complexFilter
                //{
                //    key = createdAt,
                //    value = new associativeEntity
                //    {
                //        key = "from",
                //        value = fromDate.ToString(magentoDateTimeFormat)
                //    }
                //};
                
                //cpf[1] = new complexFilter
                //{
                //    key = createdAt,
                //    value = new associativeEntity
                //    {
                //        key = "to",
                //        value = toDate.ToString(magentoDateTimeFormat)
                //    }
                //};

                //fil.complex_filter = cpf;
         

                #endregion

                #region Old way around
                string createdF = "created_at";
                string datefrom = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") + " 00:00:00" : null;
                string dateto = orderDate.HasValue ? orderDate.Value.ToString("yyyy-MM-dd") + " 23:59:59" : null;

                 associativeEntity dtFrom = new associativeEntity() { key = "from", value = datefrom };
                 associativeEntity dtTo = new associativeEntity() { key = "to", value = dateto };

                 complexFilter[] complexFil = { 
                                                    new complexFilter(){key = createdF, value = dtFrom},
                                                    new complexFilter(){key = createdF, value = dtTo}                                                 
                                                 };
                fil.complex_filter = complexFil;
                #endregion
            }

            salesOrders = proxy.salesOrderList(sessionId, fil);
            List<salesOrderListEntity> newOrders = new List<salesOrderListEntity>();

            List<string> list = new List<string>();
            list = MagentoOrderBAL.LoadOrderIds();
            bool flg = false;
            foreach (var item in salesOrders)
            {
                flg = false;
                foreach (var item2 in list)
                {
                    if (item.order_id.Equals(item2))
                    {
                        flg = true;
                        break;
                    }
                }

                if (!flg)
                {
                    newOrders.Add(item);
                }

            }

            EndSession(sessionId);

            newOrders = newOrders.Where(w => !String.IsNullOrEmpty(w.customer_id) && Convert.ToInt64(w.customer_id) > 0).ToList();

            return /*salesOrders*/ newOrders;//.ToArray();
        }

        public salesOrderListEntity[] GetSalesOrdersByOrderID(string sessionId, string order_id)
        {
            //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
            //string sessionId = proxy.login("softech", "admin123");
             salesOrderListEntity[] salesOrders = null;
             filters fil = new filters();
             associativeEntity ass = new associativeEntity() { key = "increment_id", value = order_id };
            fil.filter = new associativeEntity[] { ass };

            salesOrders = proxy.salesOrderList(sessionId, fil);
            
            //proxy.endSession(sessionId);

            return salesOrders;
        }

        public salesOrderListEntity[] GetDateWiseSalesOrders(string date)
        {           
            string sessionId = BeginSession();
             salesOrderListEntity[] salesOrders = null;
             filters fil = new filters();
             associativeEntity ass = new associativeEntity() { key = "status", value = "complete" };
            if (!string.IsNullOrEmpty(date))
            {
                 associativeEntity ass2 = new associativeEntity() { key = "created_at", value = date/*value = "2012-11-01 07:19:19"*/ };
                fil.filter = new associativeEntity[] { ass, ass2 };
            }
            else
            {
                fil.filter = new associativeEntity[] { ass };
            }

            salesOrders = proxy.salesOrderList(sessionId, fil);
            EndSession(sessionId);

            return salesOrders;
        }

        #region Commented
        //public void GetSalesInvoiceOrder()
        //{             
        //    try
        //    {
        //    salesOrderListEntity[] salesOrders = GetSalesOrders();
        //    var tenRec = salesOrders.Take(10);
        //    test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
        //    string sessionId = proxy.login("softech", "admin123");

        //    foreach (var sale in tenRec)
        //        {
        //            test_ms.filters fil = new filters();
        //            test_ms.associativeEntity ass = new associativeEntity() { key = "order_id", value = sale.order_id.ToString() };
        //            fil.filter = new associativeEntity[] { ass };

        //            salesOrderInvoiceEntity[] invoices = proxy.salesOrderInvoiceList(sessionId, fil);

        //            if (invoices != null && invoices.Count() > 0)
        //            {
        //                salesOrderInvoiceEntity entity = proxy.salesOrderInvoiceInfo(sessionId, invoices[0].increment_id);

        //                DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_INFO");

        //                DataAccessHelper.CreateInParameter(command, "?p_invoice_id", DbType.String, entity.invoice_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, entity.grand_total);
        //                DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, entity.state);
        //                DataAccessHelper.CreateInParameter(command, "?p_order_created_at", DbType.String, entity.order_created_at);
        //                DataAccessHelper.CreateInParameter(command, "?p_order_increment_id", DbType.String, entity.order_increment_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, entity.order_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, entity.billing_lastname);
        //                DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, entity.billing_firstname);
        //                DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, entity.billing_address_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, entity.base_tax_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, entity.tax_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, entity.base_shipping_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, entity.shipping_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, entity.base_discount_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, entity.discount_amount);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, entity.base_grand_total);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, entity.base_subtotal);
        //                DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, entity.subtotal);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, entity.base_to_order_rate);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, entity.base_to_global_rate);
        //                DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, entity.store_to_order_rate);
        //                DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, entity.store_to_base_rate);
        //                DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, entity.order_currency_code);
        //                DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, entity.store_currency_code);
        //                DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, entity.base_currency_code);
        //                DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, entity.global_currency_code);
        //                DataAccessHelper.CreateInParameter(command, "?p_is_active", DbType.String, entity.is_active);
        //                DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, entity.updated_at);
        //                DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, entity.created_at);
        //                DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, entity.store_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, entity.parent_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, entity.increment_id);
        //                DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
        //                DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

        //                bool data = DataAccessHelper.ExecuteNonQuery(command);


        //                foreach (salesOrderInvoiceItemEntity item in entity.items)
        //                {
        //                    DbCommand command2 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_DETAIL");

        //                    DataAccessHelper.CreateInParameter(command2,"p_item_id"         			      , DbType.String,item.item_id);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_product_id"                        , DbType.String,item.product_id);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_order_item_id"                     , DbType.String,item.order_item_id);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_name"                              , DbType.String,item.name);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_sku"                               , DbType.String,item.sku);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_weee_tax_row_disposition"     , DbType.String,item.base_weee_tax_row_disposition);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_weee_tax_disposition"         , DbType.String,item.base_weee_tax_disposition);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_weee_tax_row_disposition"          , DbType.String,item.weee_tax_row_disposition);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_weee_tax_disposition"              , DbType.String,item.weee_tax_disposition);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_weee_tax_applied_row_amount"       , DbType.String,item.weee_tax_applied_row_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_weee_tax_applied_amount"           , DbType.String,item.weee_tax_applied_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_weee_tax_applied_row_amount"  , DbType.String,item.base_weee_tax_applied_row_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_weee_tax_applied_amount"      , DbType.String,item.base_weee_tax_applied_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_row_total"                    , DbType.String,item.base_row_total);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_tax_amount"                   , DbType.String,item.base_tax_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_base_price"                        , DbType.String,item.base_price);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_row_total"                         , DbType.String,item.row_total);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_tax_amount"                        , DbType.String,item.tax_amount);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_price"                             , DbType.String,item.price);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_cost"                              , DbType.String,item.cost);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_qty"                               , DbType.String,item.qty);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_weee_tax_applied"                  , DbType.String,item.weee_tax_applied);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_is_active"                         , DbType.String,item.is_active);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_updated_at"                        , DbType.String,item.updated_at);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_created_at"                        , DbType.String,item.created_at);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_parent_id"                         , DbType.String,item.parent_id);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_increment_id"                      , DbType.String,item.increment_id);
        //                    DataAccessHelper.CreateInParameter(command2, "p_order_id", DbType.String, entity.order_id);  
        //                    DataAccessHelper.CreateInParameter(command2,"p_softech_store_id"                  , DbType.Int64,1);
        //                    DataAccessHelper.CreateInParameter(command2, "?p_procedure_date", DbType.DateTime, DateTime.Now);


        //                    bool data2 = DataAccessHelper.ExecuteNonQuery(command2);
        //                }


        //                foreach (salesOrderInvoiceCommentEntity item in entity.comments)
        //                {
        //                    DbCommand command3 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_COM_DETL");

        //                    DataAccessHelper.CreateInParameter(command3,"p_comment_id"            , DbType.String ,item.comment_id); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_is_customer_notified"  , DbType.String ,item.is_customer_notified); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_is_active"             , DbType.String ,item.is_active); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_updated_at"            , DbType.String ,item.updated_at); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_created_at"            , DbType.String ,item.created_at); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_parent_id"             , DbType.String ,item.parent_id); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_comment"               , DbType.String ,item.comment); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_increment_id"          , DbType.String ,item.increment_id); 
        //                    DataAccessHelper.CreateInParameter(command3,"p_softech_store_id"      , DbType.Int64 ,1);
        //                    DataAccessHelper.CreateInParameter(command3, "?p_procedure_date", DbType.DateTime, DateTime.Now);



        //                    bool data3 = DataAccessHelper.ExecuteNonQuery(command3);
        //                }


        //            }
        //        }

        //        proxy.endSession(sessionId);
        //    }
        //    catch (Exception ex)
        //    {
        //        bool rethrow = false;
        //        rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
        //        if (rethrow)
        //        {
        //            throw ex;
        //        }
        //    }

        //}
        #endregion

        public string GetSalesInvoiceOrder(string order_id, Int64? storeId, out string productCodes)
        {

            TimeSpan timeout = TimeSpan.FromSeconds(0);
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, timeout))
            {
                productCodes = "";
                try
                {
                    string result = "";
                    bool data = false;
                    Filters oFilter = new Filters();
                    ItemDetail oItemDetail = new ItemDetail();
                    List<ItemDetail> lItemDetail = new List<ItemDetail>();
                    salesOrderEntity soe = null;
                    string sessionId = BeginSession();
                    var saleOrder = SaveSalesOrder(sessionId, order_id);
                    //test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new test_ms.Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                    //string sessionId = proxy.login("softech", "admin123");
                     filters fil = new filters();
                     associativeEntity ass = new associativeEntity() { key = "order_id", value = saleOrder[0].order_id };
                    fil.filter = new associativeEntity[] { ass };
                    List<string> productIds = new List<string>();
                    List<string> productDesc = new List<string>();
                    List<string> quantities = new List<string>();
                    List<string> prices = new List<string>();
                    List<StoreItemMapping> newMappingItems = new List<StoreItemMapping>();
                    salesOrderInvoiceEntity[] invoices = proxy.salesOrderInvoiceList(sessionId, fil);
                    salesOrderInvoiceEntity entity = null;

                    if (invoices != null && invoices.Count() > 0)
                    {
                        entity = proxy.salesOrderInvoiceInfo(sessionId, invoices[0].increment_id);

                        SaveInvoices(entity);

                        productIds = entity.items.Select(s => s.product_id).ToList();

                        productDesc = entity.items.Select(s => s.sku).ToList();

                        quantities = entity.items.Select(s => s.qty).ToList();

                        prices = entity.items.Select(s => s.price).ToList();

                        newMappingItems = MagentoOrderBAL.MagentoProductIDExist(string.Join(",", productIds.ToArray()));

                        soe = proxy.salesOrderInfo(sessionId, saleOrder[0].increment_id);

                    }

                    #region Reverse Sales Orders

                    if (newMappingItems.Count == productIds.Count)
                    {
                        AutoSINAndSIR
                            (
                               quantities, prices, order_id, storeId, saleOrder[0].customer_id, saleOrder[0].customer_firstname + " " + saleOrder[0].customer_lastname, 
                                newMappingItems, sessionId, null, soe
                            );
                    }                    
                    else if (newMappingItems.Count != productIds.Count && productIds.Count > 0)
                    {
                        int count = newMappingItems.Where(w => w.MagentoProductID == productIds[0]).Count();
                        if (count > 0 && count == newMappingItems.Count)
                        {
                            AutoSINAndSIR
                                (
                                   quantities, prices, order_id, storeId, saleOrder[0].customer_id, saleOrder[0].customer_firstname + " " + saleOrder[0].customer_lastname,
                                    newMappingItems, sessionId, null, soe                                 
                                );
                        }
                        else
                        {

                            productCodes ="\"" + productIds[0] + " - " + productDesc[0]+ "\"";

                            data = ReverseSalesOrders(data, entity);
                            result = "101";
                        }                        
                        //  return "101";
                    }
                    else if (newMappingItems.Count == 0 && productIds.Count > 0)
                    {
                        productCodes = "\"" + productIds[0] + " - " + productDesc[0] + "\"";

                        data = ReverseSalesOrders(data, entity);
                        result = "101";
                        // return "101";
                    }

                    #endregion

                    EndSession(sessionId);

                    scope.Complete();

                    if (!result.Equals("101"))
                    {
                        result = "true";
                    }

                    //return "true";
                    return result;
                }
                catch (Exception ex)
                {
                    bool rethrow = false;
                    return "false";
                    rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                    if (rethrow)
                    {
                        throw ex;
                    }
                }
            }

        }


        public string GenerateCINThroughPhp(MagentoOrder order, List<MagentoOrderDetail> orderDets, MagentoInvoice invoice, Int64? storeId, MagentoCustomer address)
        {

            TimeSpan timeout = TimeSpan.FromSeconds(0);
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, timeout))
            {
                
                try
                {
                    string result = "";
                    bool data = false;
                    Filters oFilter = new Filters();
                    ItemDetail oItemDetail = new ItemDetail();
                    List<ItemDetail> lItemDetail = new List<ItemDetail>();

                    SaveSaleOrder(order); 
                    //test_ms.filters fil = new filters();
                    //test_ms.associativeEntity ass = new associativeEntity() { key = "order_id", value = saleOrder[0].order_id };
                    //fil.filter = new associativeEntity[] { ass };
                    List<string> productIds = new List<string>();
                    List<string> productDesc = new List<string>();
                    List<string> quantities = new List<string>();
                    List<string> prices = new List<string>();
                    List<StoreItemMapping> newMappingItems = new List<StoreItemMapping>();
                    //salesOrderInvoiceEntity[] invoices = proxy.salesOrderInvoiceList(sessionId, fil);
                    //salesOrderInvoiceEntity entity = null;

                    //if (invoices != null && invoices.Count() > 0)
                    //{
                    //    entity = proxy.salesOrderInvoiceInfo(sessionId, invoices[0].increment_id);

                        SaveInvoices(invoice, orderDets);

                        productIds = orderDets.Select(s => s.product_id).ToList();
                            //entity.items.Select(s => s.product_id).ToList();

                        productDesc = orderDets.Select(s => s.sku).ToList();
                           // entity.items.Select(s => s.sku).ToList()

                        quantities = orderDets.Select(s => s.qty).ToList();

                        prices = orderDets.Select(s => s.price).ToList();

                        newMappingItems = MagentoOrderBAL.MagentoProductIDExist(string.Join(",", productIds.ToArray()));

                    //}

                    #region Reverse Sales Orders

                    if (newMappingItems.Count == productIds.Count)
                    {
                        AutoSINAndSIR
                            (
                               quantities, prices, order.increment_id, storeId, order.customer_id, order.customer_firstname + " " + order.customer_lastname,
                                newMappingItems, "",address
                            );
                    }
                    else if (newMappingItems.Count != productIds.Count && productIds.Count > 0)
                    {
                        int count = newMappingItems.Where(w => w.MagentoProductID == productIds[0]).Count();
                        if (count > 0 && count == newMappingItems.Count)
                        {
                            AutoSINAndSIR
                                (
                                   quantities, prices, order.increment_id, storeId, order.customer_id, order.customer_firstname + " " + order.customer_lastname,
                                    newMappingItems, "", address
                                );
                        }
                        else
                        {                           

                            data = ReverseSalesOrders(data, invoice);
                            result = "101";
                        }
                        //  return "101";
                    }
                    else if (newMappingItems.Count == 0 && productIds.Count > 0)
                    {
                       

                        data = ReverseSalesOrders(data, invoice);
                        result = "101";
                        // return "101";
                    }

                    #endregion

                    

                    scope.Complete();

                    if (!result.Equals("101"))
                    {
                        result = "true";
                    }

                    //return "true";
                    return result;
                }
                catch (Exception ex)
                {
                    bool rethrow = false;
                    return "false";
                    rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                    if (rethrow)
                    {
                        throw ex;
                    }
                }
            }

        }


        public void GenerateAllSaleOrders(Int64? storeId)
        {

            TimeSpan timeout = TimeSpan.FromSeconds(0);
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew, timeout))
            {                
                try
                {
                    string result = "";
                    bool data = false;
                    Filters oFilter = new Filters();
                    ItemDetail oItemDetail = new ItemDetail();
                    List<ItemDetail> lItemDetail = new List<ItemDetail>();
                    string sessionId = BeginUSASession();
                    //var saleOrder = SaveSalesOrder(sessionId, order_id);
                    var saleOrders = MagentoOrderBAL.GetMagentoSaleOrders();
                    foreach (var saleOrder in saleOrders)
                    {

                        //if (Convert.ToInt32(saleOrder.IncrementId) > 100000473)
                        //{
                            usa_ms.filters fil = new usa_ms.filters();
                            usa_ms.associativeEntity ass = new usa_ms.associativeEntity() { key = "order_id", value = saleOrder.OrderId };
                            fil.filter = new usa_ms.associativeEntity[] { ass };
                            List<string> productIds = new List<string>();
                            List<string> productDesc = new List<string>();
                            List<string> quantities = new List<string>();
                            List<string> prices = new List<string>();
                            List<StoreItemMapping> newMappingItems = new List<StoreItemMapping>();
                            usa_ms.salesOrderInvoiceEntity[] invoices = usaProxy.salesOrderInvoiceList(sessionId, fil);
                            usa_ms.salesOrderInvoiceEntity entity = null;

                            if (invoices != null && invoices.Count() > 0)
                            {
                                entity = usaProxy.salesOrderInvoiceInfo(sessionId, invoices[0].increment_id);

                                productIds = entity.items.Select(s => s.product_id).ToList();

                                productDesc = entity.items.Select(s => s.sku).ToList();

                                quantities = entity.items.Select(s => s.qty).ToList();

                                prices = entity.items.Select(s => s.price).ToList();

                                newMappingItems = MagentoOrderBAL.MagentoProductIDExist(string.Join(",", productIds.ToArray()));

                            }

                            AutoSINAndSIR
                               (
                                      quantities, prices, saleOrder.IncrementId, storeId, saleOrder.CustomerId, saleOrder.CustomerFirstName + " " + saleOrder.CustomerLastName,
                                       newMappingItems, sessionId
                               );
                        //}
                    }
                 //}


                    EndUSASession(sessionId);

                    scope.Complete();
                    
                }
                catch (Exception ex)
                {
                    bool rethrow = false;
                    
                    rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                    if (rethrow)
                    {
                        throw ex;
                    }
                }
            }

        }

        private static bool ReverseSalesOrders(bool data, salesOrderInvoiceEntity entity)
        {
            DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_DELETE_MAGENTO_SALES_ORDER");
            DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.Int64, entity.order_id);
            data = DataAccessHelper.ExecuteNonQuery(command);
            return data;
        }

        private static bool ReverseSalesOrders(bool data, MagentoInvoice entity)
        {
            DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_DELETE_MAGENTO_SALES_ORDER");
            DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.Int64, entity.order_id);
            data = DataAccessHelper.ExecuteNonQuery(command);
            return data;
        }

        private static void SaveInvoices(salesOrderInvoiceEntity entity)
        {
            DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_INFO");

            DataAccessHelper.CreateInParameter(command, "?p_invoice_id", DbType.String, entity.invoice_id);
            DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, entity.grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, entity.state);
            DataAccessHelper.CreateInParameter(command, "?p_order_created_at", DbType.String, entity.order_created_at);
            DataAccessHelper.CreateInParameter(command, "?p_order_increment_id", DbType.String, entity.order_increment_id);
            DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, entity.order_id);
            DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, entity.billing_lastname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, entity.billing_firstname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, entity.billing_address_id);
            DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, entity.base_tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, entity.tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, entity.base_shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, entity.shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, entity.base_discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, entity.discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, entity.base_grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, entity.base_subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, entity.subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, entity.base_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, entity.base_to_global_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, entity.store_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, entity.store_to_base_rate);
            DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, entity.order_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, entity.store_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, entity.base_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, entity.global_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_is_active", DbType.String, entity.is_active);
            DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, entity.updated_at);
            DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, entity.created_at);
            DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, entity.store_id);
            DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, entity.parent_id);
            DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, entity.increment_id);
            DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
            DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

            bool data = DataAccessHelper.ExecuteNonQuery(command);


            foreach (salesOrderInvoiceItemEntity item in entity.items)
            {
                DbCommand command2 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_DETAIL");

                DataAccessHelper.CreateInParameter(command2, "p_item_id", DbType.String, item.item_id);
                DataAccessHelper.CreateInParameter(command2, "p_product_id", DbType.String, item.product_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_item_id", DbType.String, item.order_item_id);
                DataAccessHelper.CreateInParameter(command2, "p_name", DbType.String, item.name);
                DataAccessHelper.CreateInParameter(command2, "p_sku", DbType.String, item.sku);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_row_disposition", DbType.String, item.base_weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_disposition", DbType.String, item.base_weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_row_disposition", DbType.String, item.weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_disposition", DbType.String, item.weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_row_amount", DbType.String, item.weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_amount", DbType.String, item.weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_row_amount", DbType.String, item.base_weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_amount", DbType.String, item.base_weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_row_total", DbType.String, item.base_row_total);
                DataAccessHelper.CreateInParameter(command2, "p_base_tax_amount", DbType.String, item.base_tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_price", DbType.String, item.base_price);
                DataAccessHelper.CreateInParameter(command2, "p_row_total", DbType.String, item.row_total);
                DataAccessHelper.CreateInParameter(command2, "p_tax_amount", DbType.String, item.tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_price", DbType.String, item.price);
                DataAccessHelper.CreateInParameter(command2, "p_cost", DbType.String, item.cost);
                DataAccessHelper.CreateInParameter(command2, "p_qty", DbType.String, item.qty);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied", DbType.String, item.weee_tax_applied);
                DataAccessHelper.CreateInParameter(command2, "p_is_active", DbType.String, item.is_active);
                DataAccessHelper.CreateInParameter(command2, "p_updated_at", DbType.String, item.updated_at);
                DataAccessHelper.CreateInParameter(command2, "p_created_at", DbType.String, item.created_at);
                DataAccessHelper.CreateInParameter(command2, "p_parent_id", DbType.String, item.parent_id);
                DataAccessHelper.CreateInParameter(command2, "p_increment_id", DbType.String, item.increment_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_id", DbType.String, entity.order_id);
                DataAccessHelper.CreateInParameter(command2, "p_softech_store_id", DbType.Int64, 1);
                DataAccessHelper.CreateInParameter(command2, "?p_procedure_date", DbType.DateTime, DateTime.Now);


                bool data2 = DataAccessHelper.ExecuteNonQuery(command2);
            }


            foreach (salesOrderInvoiceCommentEntity item in entity.comments)
            {
                DbCommand command3 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_COM_DETL");

                DataAccessHelper.CreateInParameter(command3, "p_comment_id", DbType.String, item.comment_id);
                DataAccessHelper.CreateInParameter(command3, "p_is_customer_notified", DbType.String, item.is_customer_notified);
                DataAccessHelper.CreateInParameter(command3, "p_is_active", DbType.String, item.is_active);
                DataAccessHelper.CreateInParameter(command3, "p_updated_at", DbType.String, item.updated_at);
                DataAccessHelper.CreateInParameter(command3, "p_created_at", DbType.String, item.created_at);
                DataAccessHelper.CreateInParameter(command3, "p_parent_id", DbType.String, item.parent_id);
                DataAccessHelper.CreateInParameter(command3, "p_comment", DbType.String, item.comment);
                DataAccessHelper.CreateInParameter(command3, "p_increment_id", DbType.String, item.increment_id);
                DataAccessHelper.CreateInParameter(command3, "p_softech_store_id", DbType.Int64, 1);
                DataAccessHelper.CreateInParameter(command3, "?p_procedure_date", DbType.DateTime, DateTime.Now);



                bool data3 = DataAccessHelper.ExecuteNonQuery(command3);
            }
        }

        private static void SaveInvoices(MagentoInvoice entity, List<MagentoOrderDetail> dets)
        {
            DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_INFO");

            DataAccessHelper.CreateInParameter(command, "?p_invoice_id", DbType.String, entity.invoice_id);
            DataAccessHelper.CreateInParameter(command, "?p_grand_total", DbType.String, entity.grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_state", DbType.String, entity.state);
            DataAccessHelper.CreateInParameter(command, "?p_order_created_at", DbType.String, entity.order_created_at);
            DataAccessHelper.CreateInParameter(command, "?p_order_increment_id", DbType.String, entity.order_increment_id);
            DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, entity.order_id);
            DataAccessHelper.CreateInParameter(command, "?p_billing_lastname", DbType.String, entity.billing_lastname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_firstname", DbType.String, entity.billing_firstname);
            DataAccessHelper.CreateInParameter(command, "?p_billing_address_id", DbType.String, entity.billing_address_id);
            DataAccessHelper.CreateInParameter(command, "?p_base_tax_amount", DbType.String, entity.base_tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_tax_amount", DbType.String, entity.tax_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_shipping_amount", DbType.String, entity.base_shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_shipping_amount", DbType.String, entity.shipping_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_discount_amount", DbType.String, entity.base_discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_discount_amount", DbType.String, entity.discount_amount);
            DataAccessHelper.CreateInParameter(command, "?p_base_grand_total", DbType.String, entity.base_grand_total);
            DataAccessHelper.CreateInParameter(command, "?p_base_subtotal", DbType.String, entity.base_subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_subtotal", DbType.String, entity.subtotal);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_order_rate", DbType.String, entity.base_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_base_to_global_rate", DbType.String, entity.base_to_global_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_order_rate", DbType.String, entity.store_to_order_rate);
            DataAccessHelper.CreateInParameter(command, "?p_store_to_base_rate", DbType.String, entity.store_to_base_rate);
            DataAccessHelper.CreateInParameter(command, "?p_order_currency_code", DbType.String, entity.order_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_store_currency_code", DbType.String, entity.store_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_base_currency_code", DbType.String, entity.base_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_global_currency_code", DbType.String, entity.global_currency_code);
            DataAccessHelper.CreateInParameter(command, "?p_is_active", DbType.String, entity.is_active);
            DataAccessHelper.CreateInParameter(command, "?p_updated_at", DbType.String, entity.updated_at);
            DataAccessHelper.CreateInParameter(command, "?p_created_at", DbType.String, entity.created_at);
            DataAccessHelper.CreateInParameter(command, "?p_store_id", DbType.String, entity.store_id);
            DataAccessHelper.CreateInParameter(command, "?p_parent_id", DbType.String, entity.parent_id);
            DataAccessHelper.CreateInParameter(command, "?p_increment_id", DbType.String, entity.increment_id);
            DataAccessHelper.CreateInParameter(command, "?p_softech_store_id", DbType.Int64, 1);
            DataAccessHelper.CreateInParameter(command, "?p_procedure_date", DbType.DateTime, DateTime.Now);

            bool data = DataAccessHelper.ExecuteNonQuery(command);


            foreach (var item in dets)
            {
                DbCommand command2 = DataAccessHelper.CreateStoredProcCommand("SP_INSERT_MAGENTO_SALES_INVOICE_DETAIL");

                DataAccessHelper.CreateInParameter(command2, "p_item_id", DbType.String, item.item_id);
                DataAccessHelper.CreateInParameter(command2, "p_product_id", DbType.String, item.product_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_item_id", DbType.String, item.order_item_id);
                DataAccessHelper.CreateInParameter(command2, "p_name", DbType.String, item.name);
                DataAccessHelper.CreateInParameter(command2, "p_sku", DbType.String, item.sku);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_row_disposition", DbType.String, item.base_weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_disposition", DbType.String, item.base_weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_row_disposition", DbType.String, item.weee_tax_row_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_disposition", DbType.String, item.weee_tax_disposition);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_row_amount", DbType.String, item.weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied_amount", DbType.String, item.weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_row_amount", DbType.String, item.base_weee_tax_applied_row_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_weee_tax_applied_amount", DbType.String, item.base_weee_tax_applied_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_row_total", DbType.String, item.base_row_total);
                DataAccessHelper.CreateInParameter(command2, "p_base_tax_amount", DbType.String, item.base_tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_base_price", DbType.String, item.base_price);
                DataAccessHelper.CreateInParameter(command2, "p_row_total", DbType.String, item.row_total);
                DataAccessHelper.CreateInParameter(command2, "p_tax_amount", DbType.String, item.tax_amount);
                DataAccessHelper.CreateInParameter(command2, "p_price", DbType.String, item.price);
                DataAccessHelper.CreateInParameter(command2, "p_cost", DbType.String, item.cost);
                DataAccessHelper.CreateInParameter(command2, "p_qty", DbType.String, item.qty);
                DataAccessHelper.CreateInParameter(command2, "p_weee_tax_applied", DbType.String, item.weee_tax_applied);
                DataAccessHelper.CreateInParameter(command2, "p_is_active", DbType.String, item.is_active);
                DataAccessHelper.CreateInParameter(command2, "p_updated_at", DbType.String, item.updated_at);
                DataAccessHelper.CreateInParameter(command2, "p_created_at", DbType.String, item.created_at);
                DataAccessHelper.CreateInParameter(command2, "p_parent_id", DbType.String, item.parent_id);
                DataAccessHelper.CreateInParameter(command2, "p_increment_id", DbType.String, item.increment_id);
                DataAccessHelper.CreateInParameter(command2, "p_order_id", DbType.String, entity.order_id);
                DataAccessHelper.CreateInParameter(command2, "p_softech_store_id", DbType.Int64, 1);
                DataAccessHelper.CreateInParameter(command2, "?p_procedure_date", DbType.DateTime, DateTime.Now);


                bool data2 = DataAccessHelper.ExecuteNonQuery(command2);
            }
        }


        public List<StoreItemMapping> GetStoreItemMapping()
        {
            try
            {
                return StoreItemMappingBAL.LoadStoreItemMapping();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool SaveMagentoTransaction(StoreTransaction trans, List<StoreTransactionDetail> listTrans)
        {
            try
            {
                #region Commendted on Dated:<17Mar2014>
                //if(magentoTransactions == null || magentoTransactions.Count <= 0)
                //{
                //  foreach (StoreItemMapping item in GetStoreItemMapping())
                //  {
                //      HFMagentoTransaction obj = new HFMagentoTransaction() 
                //      { 
                //          MasterOid = item.MasterOid,
                //          DetailOid = item.DetailOid,
                //          InvItemDetail = item.ItemOid,
                //          ProductId = item.ProductId,
                //          Qty = item.Qty,
                //          TransCode = item.TransCode,
                //          TransDate = item.TransDate
                //      };
                //    magentoTransactions.Add(obj);
                //   HFMagentoTransactionBAL.SaveMagentoTransaction(obj);
                //  }
                //}
                #endregion

                foreach (var item in listTrans)
                {
                    string productId = ItemMappingBAL.LoadItemMapping(item.InvItemDetail);

                    HFMagentoTransaction obj = new HFMagentoTransaction()
                    {
                        MasterOid = item.InvStoresTransaction,
                        DetailOid = item.Oid,
                        InvItemDetail = item.InvItemDetail,
                        ProductId = productId,
                        Qty = item.Qty,
                        TransCode = "STR-" + item.Oid, //item.TransCode,
                        TransDate = trans.TransDate
                    };
                    magentoTransactions.Add(obj);
                    HFMagentoTransactionBAL.SaveMagentoTransaction(obj);
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw ex;
            }
        }

        public List<HFMagentoTransaction> ReadMagentoTransactions(StoreTransaction trans, List<StoreTransactionDetail> listTrans)
        {
            try
            {
                // to do: re-write
                //if (magentoTransactions == null || magentoTransactions.Count <= 0)
                //    magentoTransactions = HFMagentoTransactionBAL.LoadMagentoTrnasactions();

                //if (magentoTransactions == null || magentoTransactions.Count <= 0)
                SaveMagentoTransaction(trans, listTrans);


                return magentoTransactions;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        private string BeginSession()
        {
            return proxy.login("softech", "admin123"); 
        }

        private void EndSession(string sessionId)
        {
            proxy.endSession(sessionId);
        }

        private string BeginUSASession()
        {
            return usaProxy.login("softech", "admin123");
        }

        private void EndUSASession(string sessionId)
        {
            usaProxy.endSession(sessionId);
        }

        public catalogInventoryStockItemEntity[] ReadMagentoStock(string sessionId,StoreTransaction trans, List<StoreTransactionDetail> listTrans)
        {
            try
            {

                //string sessionId = BeginSession();
                List<HFMagentoTransaction> transactions = ReadMagentoTransactions(trans, listTrans);

                string[] productIds = transactions.Select(s => s.ProductId).ToArray();
                catalogInventoryStockItemEntity[] stocks;
                stocks = proxy.catalogInventoryStockItemList(sessionId, productIds);
                //EndSession(sessionId);
                return stocks;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool UpdateMagentoStock(StoreTransaction trans, List<StoreTransactionDetail> listTrans, bool IsAdd = true,string p_sessionId = "")
        {
            try
            {
                catalogInventoryStockItemEntity[] stocks = null;
                string sessionID = "";
                if (string.IsNullOrEmpty(p_sessionId))
                {
                    sessionID = BeginSession();
                    stocks = ReadMagentoStock(sessionID, trans, listTrans);
                }
                else
                {
                    stocks = ReadMagentoStock(p_sessionId, trans, listTrans);
                }
                catalogInventoryStockItemUpdateEntity obj = new catalogInventoryStockItemUpdateEntity();

                #region Commented
                //var qty = from m in magentoTransactions
                //             join st in stocks on m.ProductId equals st.product_id
                //             select new { quantity = m.Qty + st.qty };
                #endregion

                if (stocks != null && magentoTransactions != null && stocks.Count() > 0 && magentoTransactions.Count > 0)
                {
                    foreach (var item in magentoTransactions)
                    {

                        var quantity = stocks.Where(w => w.product_id.Equals(item.ProductId))
                                             .FirstOrDefault();

                        if (quantity != null && item != null)
                        {
                            if (IsAdd)
                            {
                                obj.qty = Convert.ToString(Convert.ToDouble(quantity.qty) + Convert.ToDouble(item.Qty));
                               
                            }
                            else
                            {
                                if (Convert.ToDouble(quantity.qty) <= 0)
                                {
                                    obj.qty = "0";
                                }
                                else
                                {
                                    obj.qty = Convert.ToString(Convert.ToDouble(quantity.qty) - Convert.ToDouble(item.Qty));
                                }
                                
                                
                            }
                            

                            // if(trans.InvStores == 1)                               
                            //Mage_Api_Model_Server_V2_HandlerPortTypeClient proxy = new Mage_Api_Model_Server_V2_HandlerPortTypeClient();
                            //string sessionId = proxy.login("softech", "admin123");

                            if (string.IsNullOrEmpty(p_sessionId))
                            {
                                proxy.catalogInventoryStockItemUpdate(sessionID, item.ProductId, obj);
                            }
                            else
                            {
                                proxy.catalogInventoryStockItemUpdate(p_sessionId, item.ProductId, obj);
                            }
                            //proxy.endSession(sessionId);
                        }

                    }

                }
                if (string.IsNullOrEmpty(p_sessionId))
                {
                    EndSession(sessionID);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
                //throw ex;
            }

        }

        public List<MagentoOrderDetail> TestService(MagentoOrder order, List<MagentoOrderDetail> orderDets)
        {
            try
            {

                return orderDets;
            }
            catch (Exception)
            {
                return null;
                //throw;
            }
        }

        public bool MagentoOrderUpdateInInventory(MagentoOrder order, List<MagentoOrderDetail> orderDets)
        {
            try
            {


                //AutoSINAndSIR(/*order, orderDets*/);


                return true;
            }
            catch (Exception ex)
            {

                return false;
                //throw;
            }

        }

        public static bool UpdateMagentoSalesOrder(Int64? cinOid, string orderId)
        {
            try
            {
                DbCommand command = DataAccessHelper.CreateStoredProcCommand("SP_UPDATE_MAGENTO_ORDERS_CIN");

                DataAccessHelper.CreateInParameter(command, "?p_cin_oid", DbType.Int64, cinOid);
                DataAccessHelper.CreateInParameter(command, "?p_order_id", DbType.String, orderId);
                bool data = DataAccessHelper.ExecuteNonQuery(command);
                return data;


            }
            catch (Exception ex)
            {
                bool rethrow = false;
                rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                if (rethrow)
                {
                    throw ex;
                }
                return false;
            }
        }
        private void AutoSINAndSIR(List<string> quantities, List<string> prices, string order_id, Int64? storeId, string customerID, 
            string customerName, List<StoreItemMapping> items, string sessionId, MagentoCustomer address = null, 
            salesOrderEntity soe = null
            /*MagentoOrder order, List<MagentoOrderDetail> orderDets*/)
        {
            string msg = "";
            //JsResult jsr = new JsResult();
            //jsr.message = msg;
            //jsr.success = "false";
            try
            {
                //TimeSpan timeout = TimeSpan.FromSeconds(0);
                //using (var scope = new TransactionScope(TransactionScopeOption.Required,timeout))
                //{
                    Int64? org = 15706;
                    InvSystem oInvsystem = new InvSystem();
                    List<InvSystem> lInvSystem = new List<InvSystem>();
                    Filters oFilter = new Filters();                

                    oFilter.AddParameters(() => oInvsystem.Organisation, OperatorsList.Equal, org);


                    lInvSystem = InvSystemBAL.LoadSystems(oInvsystem, oFilter);


                    string SIRCode = StoreTransactionBAL.GetMaxCode(lInvSystem[0].SirActivity, org /*MySession.Current.DefaultOrganization.Oid*/);
                    StoreTransaction st = new StoreTransaction();
                    st.TransCode = SIRCode;
                    st.InvStores = storeId;//1; //long.Parse(frm["InvSaleStores"].ToString());
                    st.InvActivity = lInvSystem[0].SirActivity;
                    st.Department = lInvSystem[0].DefaultDepartment;//1; //oStoresCostCenterMaping.storeCoreErpCostCenter;  //16;
                    st.CustomerNameForCashSale = customerName;

                    st.InvTransactionType = lInvSystem[0].SirActivity;

                    st.MRNNo = order_id; // frm["txtMR"].ToString();
                    st.Organization = org; // MySession.Current.DefaultOrganization.Oid;
                    st.Post = true;
                    st.PostDate = DateTime.Now;
                    st.TransDate = DateTime.Now;
                    Customer cus = new Customer();
                    Filters ofilter = new Filters();
                    ofilter.AddParameters(()=> cus.MagentoCustomerId, OperatorsList.Equal,customerID );

                    List<Customer>  lCustomers = CustomerBAL.LoadCustomer(cus,ofilter);                                        

                    if (lCustomers != null && lCustomers.Count > 0)
                    {
                        if (address != null)
                        {
                            lCustomers[0].ShippingAddress = address.shippingstreet;
                            lCustomers[0].ShippingState = address.shippingregion;
                            lCustomers[0].ShippingCity = address.shippingcity;
                            lCustomers[0].ShippingZip = address.shippingpostcode;
                            lCustomers[0].ShippingCountry = address.shippingcountry_id;

                            lCustomers[0].BillingAddress = address.billingstreet;
                            lCustomers[0].BillingState = address.billingregion;
                            lCustomers[0].BillingCity = address.billingcity;
                            lCustomers[0].BillingZip = address.billingpostcode;
                            lCustomers[0].BillingCountry = address.billingcountry_id;

                            lCustomers[0].Store = storeId;

                            lCustomers[0].Email = address.CustomerEmail;
                        }
                        else
                        {
                            var ship = soe.shipping_address;
                            lCustomers[0].ShippingAddress = ship.street;
                            lCustomers[0].ShippingState = ship.region;
                            lCustomers[0].ShippingCity = ship.city;
                            lCustomers[0].ShippingZip = ship.postcode;
                            lCustomers[0].ShippingCountry = ship.country_id;

                            lCustomers[0].Email = soe.customer_email;

                            var bill = soe.billing_address;
                            lCustomers[0].BillingAddress = bill.street;
                            lCustomers[0].BillingState = bill.region;
                            lCustomers[0].BillingCity = bill.city;
                            lCustomers[0].BillingZip = bill.postcode;
                            lCustomers[0].BillingCountry = bill.country_id;
                            lCustomers[0].Store = storeId;

                        }


                        lCustomers[0].MagentoCustomerId = customerID;
                        lCustomers[0].CustomerName = customerName;
                        CustomerBAL.UpdateCustomer(lCustomers[0]);
                    }
                    else
                    {
                        if (address != null)
                        {
                            cus.ShippingAddress = address.shippingstreet;
                            cus.ShippingState = address.shippingregion;
                            cus.ShippingCity = address.shippingcity;
                            cus.ShippingZip = address.shippingpostcode;
                            cus.ShippingCountry = address.shippingcountry_id;

                            cus.BillingAddress = address.billingstreet;
                            cus.BillingState = address.billingregion;
                            cus.BillingCity = address.billingcity;
                            cus.BillingZip = address.billingpostcode;
                            cus.BillingCountry = address.billingcountry_id;

                            cus.Store = storeId;
                            cus.Email = address.CustomerEmail;
                        }
                        else
                        {

                            var ship = soe.shipping_address;
                            cus.ShippingAddress = ship.street;
                            cus.ShippingState = ship.region;
                            cus.ShippingCity = ship.city;
                            cus.ShippingZip = ship.postcode;
                            cus.ShippingCountry = ship.country_id;

                            cus.Email = soe.customer_email;

                            var bill = soe.billing_address;
                            cus.BillingAddress = bill.street;
                            cus.BillingState = bill.region;
                            cus.BillingCity = bill.city;
                            cus.BillingZip = bill.postcode;
                            cus.BillingCountry = bill.country_id;
                            cus.Store = storeId;
                        }
                        cus.MagentoCustomerId = customerID;
                        cus.CustomerName = customerName;
                        cus.ParentCode = lInvSystem[0].DefaultDistributor;
                        cus.CustomerLevel = 3;
                        cus.Organization = 15706;
                        cus = CustomerBAL.SetCustomerCode(cus);
                        
                        CustomerBAL.saveCustomer(cus);

                    }
                      

                    st.MagentoCustomerID = customerID;

                    //st.Currency = order.Currency;

                    //string[] ids = frm["hdnID"].ToString().Split(new char[] { ',' });




                    List<StoreTransactionDetail> stdList = new List<StoreTransactionDetail>();

                    //List<StoreItemMapping> items =  GetStoreItemMapping();
                    int index = 0;
                    foreach (var o in items)
                    {
                        //if (frm["ItemOid_" + i].ToString().Trim() != "")
                        //{                       
                         
                        StoreTransactionDetail std = new StoreTransactionDetail();
                        //var itemOid = items.Where(w => w!= null && w.ProductId.Equals(o.ProductID)).Select(f => f.ItemOid);
                        std.InvItemDetail = o.InvItemDetail; //1; //o.ProductID.HasValue ? ItemMappingBAL.LoadSpecificItem(o.ProductID.Value.ToString()) : 0; //long.Parse(frm["ItemOid_" + i].ToString());



                        Int64? itemUnit = MagentoOrderBAL.LoadItemUnit(o.InvItemDetail); //1; //std.InvItemDetail.HasValue ? ItemMappingBAL.LoadItemUnit(std.InvItemDetail) : 0;

                        std.InvItemUnit = itemUnit;


                        //std.InvItemUnit = 1; //long.Parse(frm["itemUnit_" + i].ToString());
                        //long qty = 1; //Convert.ToInt64(o.Qty);  // long.Parse(frm["txtQty" + i].ToString());
                        std.RequestedQty = Convert.ToDouble(quantities[index]);
                        std.Qty = Convert.ToDouble(quantities[index]);
                        std.RetailPrice = Convert.ToDouble(prices[index]);
                        std.IsHODApprovalReq = 0;
                        std.HODApprovalStatus = 0;
                        std.IsStoreEffective = 1;
                        std.StoreEffectValue = 0;
                        stdList.Add(std);
                        ++index;
                    }                    

                    long a = SaveTransaction(st, stdList);
                    UpdateMagentoSalesOrder(/*st.Oid*/ a, order_id);
                    //if (st.Post)
                    //{
                    //    UpdateMagentoStock(st, stdList, false,sessionId);

                    //}                    
                    
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }


        public long SaveTransaction(StoreTransaction p_oStoreTransactions, List<StoreTransactionDetail> p_lStoreTransactionDetail)
        {
            try
            {
                List<StoreTransaction> lStoreTransaction = new List<StoreTransaction>();
                StoreTransaction oStoreTransaction = new StoreTransaction();
                StoreTransactionDetail oStoreTransactionDet = new StoreTransactionDetail();
                StoreTransactionDetailBalance oStoreTransDetailBal = new StoreTransactionDetailBalance();
                Int64? org = 15706;
                InvSystem oInvsystem = new InvSystem();
                List<InvSystem> lInvSystem = new List<InvSystem>();
                Filters oFilter = new Filters();

                oFilter.AddParameters(() => oInvsystem.Organisation, OperatorsList.Equal, org);

                lInvSystem = InvSystemBAL.LoadSystems(oInvsystem, oFilter);



                long? retID = 0;

                if (p_oStoreTransactions == null)
                    throw new BusinessLogicCustomException("Object is not instantiated in Client Save Method");
                Filters filter = new Filters();
                bool a = false;
                string msg = "";
                long Oid = 0;

                Int64 ogk_StoreTransaction = 0;
                Int64 st_AU = 0;
                // save or update code here
                try
                {
                    //a = DataAccessHelper.SaveObject(p_oStoreTransactions, 21);
                    p_oStoreTransactions.InsertCreationDate = DateTime.Now;
                    p_oStoreTransactions.InsertUserID = Convert.ToInt64(1/*MySession.Current.UserOid*/);
                    object outoGenratedKey = DataAccessHelper.SaveObjectAndGetOutgenratedKey(p_oStoreTransactions, 46);

                    ogk_StoreTransaction = Convert.ToInt64(outoGenratedKey);

                    retID = ogk_StoreTransaction;
                    ///////////////////////////////////////////////////////////////////////////////////////////////////

                    st_AU = StoreTransactionBAL.saveStoreTransaction_AU(p_oStoreTransactions, ogk_StoreTransaction, Convert.ToInt64(/*MySession.Current.UserOid*/1));

                    ///////////////////////////////////////////////////////////////////////////////////////////////////
                }
                catch (Exception ex)
                {

                    if (ex.Message.StartsWith("Duplicate"))
                    {

                        throw new DataAccessCustomException(ex.Message);

                    }
                    return 0;

                }
                //if (a == true && p_lStoreTransactionDetail.Count > 0)
                if (ogk_StoreTransaction > 0 && p_lStoreTransactionDetail.Count > 0)
                {
                    //if (!string.IsNullOrEmpty(p_oStoreTransactions.TransCode))
                    //    filter.AddParameters(() => oStoreTransaction.TransCode, OperatorsList.Equal, p_oStoreTransactions.TransCode);
                    //filter.AddParameters(() => oStoreTransaction.Users, OperatorsList.Equal, p_oStoreTransactions.Users);
                    //lStoreTransaction = DataAccessHelper.LoadObject(p_oStoreTransactions, filter).Cast<StoreTransaction>().ToList();
                    foreach (StoreTransactionDetail d in p_lStoreTransactionDetail)
                    {
                        //d.InvStoresTransaction = lStoreTransaction[0].Oid;
                        d.InvStoresTransaction = ogk_StoreTransaction;
                        // =========== For SIr approval ================ 
                        if (d.IsHODApprovalReq == 1 && p_oStoreTransactions.InvActivity == lInvSystem[0].SirActivity)
                            d.HODApprovalStatus = 0;
                        //if (d.IsHODApprovalReq == 1 && p_oStoreTransactions.InvActivity == MySession.Current.InvSystem[0].SirActivity && p_oStoreTransactions.MRNNo <>"")
                        //{
                        //  d.IsHODApprovalReq = 0;
                        // d.HODApprovalStatus = 0;
                        //}
                        // ================================================
                        if (d.InvPO == 0)
                            d.InvPO = null;

                        //Int64 st_DET = Convert.ToInt64(DataAccessHelper.SaveObjectAndGetOutgenratedKey(d, 19));
                        StoreTransactionDetail_to_save d_to_save = StoreTransactionBAL.ST_DET_TO_SAVE(d);
                        d_to_save.StoresTransaction_AU = st_AU;

                        Int64 st_DET = Convert.ToInt64(DataAccessHelper.SaveObjectAndGetOutgenratedKey(d_to_save, 22));

                        //a = DataAccessHelper.SaveObject(d, 14);
                        if (st_DET > 0)
                        {
                            /////////////////////////////////////////////////////////
                            //  a = StoreTransactionBAL.saveSTDET_AU(d, st_DET, st_AU, ogk_StoreTransaction, Convert.ToInt64(MySession.Current.UserOid));
                            //////////////////////////////////////////////////////

                        }
                        if (p_lStoreTransactionDetail[0].IsStoreEffective == 1 && p_lStoreTransactionDetail[0].StoreEffectValue == 0)
                        {
                            //if (d.InvPO != null)
                            //{
                            //    StoreTransaction oST = new StoreTransaction();
                            //    SessionHelper oSessionHelper = new SessionHelper();
                            //    long? SRN_Id = StoreTransactionBAL.InsertConsinement((long)d.InvStoresTransaction, (long)d.InvPO, (long)d.InvItemDetail, (double)d.Qty, (long)p_oStoreTransactions.Organization);
                            //    // Load data on the basis of SRN OID
                            //    Filters oFilter = new Filters();
                            //    oFilter.AddParameters(() => oST.Oid, OperatorsList.Equal, SRN_Id);
                            //    oFilter.AddParameters(() => oST.Users, OperatorsList.Equal, p_oStoreTransactions.Users);

                            //    List<StoreTransaction> svcStoreTrans = DataAccessHelper.LoadObject(oST, oFilter).Cast<StoreTransaction>().ToList();
                            //    svcStoreTrans[0].PartyGLCode = p_oStoreTransactions.PartyGLCode;
                            //    svcStoreTrans[0].costCenter = p_oStoreTransactions.costCenter;
                            //    svcStoreTrans[0].TaxGLCode = p_oStoreTransactions.TaxGLCode;
                            //    svcStoreTrans[0].FreightGLCode = p_oStoreTransactions.FreightGLCode;
                            //    // svcStoreTrans[0].VendorCode = p_oStoreTransactions.VendorCode;

                            //    oFilter = new Filters();
                            //    oFilter.AddParameters(() => oStoreTransactionDet.InvStoresTransaction, OperatorsList.Equal, SRN_Id);
                            //    List<StoreTransactionDetail> svcStoreTransDet = DataAccessHelper.LoadObject(oStoreTransactionDet, oFilter).Cast<StoreTransactionDetail>().ToList();
                            //    if (MySession.Current.InvSystem[0].EnableGLIntegration == true)
                            //    {
                            //        svcStoreTransDet = SessionHelper.GetItemGLCode(svcStoreTransDet);//(List<StoreTransactionDetail>)Session["lStoreTransDetail"];  
                            //        svcStoreTransDet = SessionHelper.GetTaxValue(svcStoreTransDet, (long)svcStoreTrans[0].InvPO);
                            //        msg = oSessionHelper.SaveJournalVoucher(svcStoreTransDet, svcStoreTrans[0], svcStoreTrans[0].PartyGLCode, svcStoreTrans[0].costCenter);
                            //    }
                            //    if (svcStoreTrans[0].PartyGLCode != "" && MySession.Current.InvSystem[0].EnablePmtIntegration == true)
                            //        Oid = oSessionHelper.SaveBillsToPayable(svcStoreTransDet, svcStoreTrans[0], svcStoreTrans[0].PartyGLCode, svcStoreTrans[0].costCenter, (long)MySession.Current.InvSystem[0].VendorPayableBillType);


                            //}

                            // Get currently saved detail Oid for insertion in detail balance
                            StoreTransactionDetail oStoreTransDetail = new StoreTransactionDetail();
                            List<StoreTransactionDetail> lStoreTransDetail = new List<StoreTransactionDetail>();
                            filter = new Filters();
                            //filter.AddParameters(() => oStoreTransDetail.InvStoresTransaction, OperatorsList.Equal, lStoreTransaction[0].Oid);
                            filter.AddParameters(() => oStoreTransDetail.InvStoresTransaction, OperatorsList.Equal, ogk_StoreTransaction);

                            filter.AddParameters(() => oStoreTransDetail.InvItemDetail, OperatorsList.Equal, d.InvItemDetail);
                            filter.AddParameters(() => oStoreTransDetail.InvItemUnit, OperatorsList.Equal, d.InvItemUnit);
                            lStoreTransDetail = DataAccessHelper.LoadObject(oStoreTransDetail, filter).Cast<StoreTransactionDetail>().ToList();

                            // Detail Child insertion for issuance type transactions

                            foreach (StoreTransactionDetail dd in lStoreTransDetail)
                            {

                                List<BatchItemTemp2> lBatchItem = new List<BatchItemTemp2>();
                                List<StoreTransactionDetailBalance> lStoreTransDetailBalance = new List<StoreTransactionDetailBalance>();

                                lBatchItem = StoreTransactionDetailBAL.GetItemByExpiry(d.InvItemDetail, p_oStoreTransactions.InvStores, d.InvItemUnit, dd.Qty, d.AvailableQty);
                                if (lBatchItem != null && lBatchItem.Count > 0)
                                {

                                    foreach (BatchItemTemp2 bal in lBatchItem)
                                    {
                                        StoreTransactionDetailBalance oStoreTransB = new StoreTransactionDetailBalance();
                                        oStoreTransB.InvStoreTransaction = ogk_StoreTransaction;// lStoreTransaction[0].Oid;
                                        oStoreTransB.InvStoreTransDetail = dd.Oid;
                                        oStoreTransB.InvItemDetail = bal.InvItemDetail;


                                        oStoreTransB.InvItemUnit = bal.InvItemUnit;
                                        oStoreTransB.UnitName = bal.UnitName;
                                        oStoreTransB.Expiry = bal.ExpiryDate;
                                        oStoreTransB.BatchNo = bal.BatchNo;
                                        oStoreTransB.ProductCode = bal.ProductCode;

                                        oStoreTransB.ProductDesc = bal.ProductDescription.Replace("'", "\'"); ;

                                        oStoreTransB.Qty = Convert.ToDouble(bal.Qty);
                                        a = DataAccessHelper.SaveObject(oStoreTransB, 11);

                                    }

                                }

                            }

                        }
                    }



                }


                return retID.Value;
            }
            catch (Exception ex)
            {
                //bool rethrow = false;
                //rethrow = BusinessLogicExceptionHandler.HandleExcetion(ref ex);
                //if (rethrow)
                // {
                throw ex;
                //}
                return 0;
            }
        }






        //public CompositeType GetDataUsingDataContract(CompositeType composite)
        //{
        //    if (composite == null)
        //    {
        //        throw new ArgumentNullException("composite");
        //    }
        //    if (composite.BoolValue)
        //    {
        //        composite.StringValue += "Suffix";
        //    }
        //    return composite;
        //}
    }
}

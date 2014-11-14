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

namespace DynamicService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IDynamicService
    {

        [OperationContract]
        void GetData(int value);

        [OperationContract]
        void InitializeProducts();

        [OperationContract]
        void InitializeCustomer();

        [OperationContract]
        void InitializeSalesOrderList();

        [OperationContract]
        void GenerateAllSaleOrders(Int64? storeId);

        [OperationContract]
        catalogProductEntity[] GetProducts(Int64 storeId);

        [OperationContract]
        int CreateProduct(string sku, string desc, string shortDesc, string price, string weight,string qty);

        [OperationContract]
        bool UpdateProduct(string productId, string price, string weight);

        [OperationContract]
        string GenerateCINThroughPhp(MagentoOrder order, List<MagentoOrderDetail> orderDets, MagentoInvoice invoice, Int64? storeId, MagentoCustomer address);

        [OperationContract]
        List<StoreItemMapping> GetStoreItemMapping();

        [OperationContract]
        List<HFMagentoTransaction> ReadMagentoTransactions(StoreTransaction trans, List<StoreTransactionDetail> listTrans);

        [OperationContract]
        bool SaveMagentoTransaction(StoreTransaction trans, List<StoreTransactionDetail> listTrans);

        [OperationContract]
        catalogInventoryStockItemEntity[] ReadMagentoStock(string sessionId, StoreTransaction trans, List<StoreTransactionDetail> listTrans);

        [OperationContract]
        bool UpdateMagentoStock(StoreTransaction trans, List<StoreTransactionDetail> listTrans, bool IsAdd = true, string p_sessionId = "");

        [OperationContract]
        bool MagentoOrderUpdateInInventory(MagentoOrder order, List<MagentoOrderDetail> orderDets);

        [OperationContract]
        List<MagentoOrderDetail> TestService(MagentoOrder order, List<MagentoOrderDetail> orderDets);

        [OperationContract]
        List<salesOrderListEntity> GetSalesOrders(DateTime? orderDate, string orderID);

        [OperationContract]
        salesOrderListEntity[] GetSalesOrdersByOrderID(string sessionId, string order_id);

        [OperationContract]
        customerCustomerEntity[] GetCustomers();

        [OperationContract]
        void SaveCustomers();

        //[OperationContract]
        //void SaveSalesOrder();

        //[OperationContract]
        //void GetSalesInvoiceOrder();

        [OperationContract]
        string GetSalesInvoiceOrder(string order_id, Int64? storeId, out string productCodes);

        [OperationContract]
        salesOrderListEntity[] GetDateWiseSalesOrders(string date);


        //[OperationContract]
        //CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}

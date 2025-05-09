<template>
    <div class="label-container">
      <div class="label-content">
        <!-- 表格样式 -->
        <table>
          <tr>
            <td colspan="4" class="header">成品包装标识卡<br>Finished Product Packaging Label</td>
          </tr>
          <tr>
            <td>编号<br>Code</td>
            <td>{{ labelData.code }}</td>
            <td>客户名称<br>Customer Name</td>
            <td>{{ labelData.customerName }}</td>
          </tr>
          <tr>
            <td>存货编码<br>Stock Code</td>
            <td>{{ labelData.stockCode }}</td>
            <td>存货名称<br>Stock Name</td>
            <td>{{ labelData.stockName }}</td>
          </tr>
          <tr>
            <td colspan="1">产品图号<br>Part No.</td>
            <td colspan="3">{{ labelData.partNo }}</td>
          </tr>
          <tr>
            <td >数量<br>Quantity</td>
            <td >{{ labelData.quantity }}</td>
            <td>箱号<br>Packing No.</td>
            <td colspan="3">{{ labelData.packingNo }}</td>
          </tr>
          <tr>
            <td>产品ID号<br>Part Serial No.</td>
            <td colspan="3">{{ labelData.partSerialNo }}</td>
          </tr>
          <tr>
            <td class="qr-code" rowspan="2" colspan="2">
              <img :src="qrCodeUrl" alt="QR Code" />
            </td>
            <td>包装日期<br>Packing Date</td>
            <td>{{ labelData.packingDate }}</td>
          </tr>
        </table>
      </div>
      <!-- 打印按钮 -->
      <button @click="printLabel" class="print-button">打印标签</button>
    </div>
  </template>
  
  <script>
  import QRCode from "qrcode"
  
  export default {
    name: "LabelPrinter",
    data() {
      return {
        labelData: {
          code: "12",
          customerName: "wabtec",
          stockCode: "100106001",
          stockName: "牵引齿轮",
          partNo: "41D735826P2",
          quantity: "4",
          packingNo: "231213-012",
          partSerialNo: "2307048/2307049/2307046/2307047",
          packingDate: "2024-10-26",
        },
        qrCodeUrl: "", // 二维码 URL
      };
    },
    methods: {
      generateQRCode() {
        // 生成二维码
        QRCode.toDataURL(this.labelData.partSerialNo)
          .then((url) => {
            this.qrCodeUrl = url;
          })
          .catch((err) => {
            console.error("二维码生成失败:", err);
          });
      },
      printLabel() {
        window.print();
      },

      // async printLabels() {
      //       if (!this.tableData || this.tableData.length === 0) {
      //           this.$message.error("没有可打印的数据！");
      //           return;
      //       }

      //       // 生成打印数据
      //       const templateItems = this.tableData.map(async (row, index) => ({
      //           index: index + 1,
      //           boxNumber: row.boxNumber,
      //           customerName: row.customerName || "",
      //           invCode: row.invCode,
      //           invName: row.invName || "",
      //           pictureCode: row.pictureCode,
      //           quantity: row.details.length,
      //           createdTime: row.createdTime.split(" ")[0],
      //           productIds: row.details.map((d) => d.flowCard).join("/"),
      //           qrCodeUrl: await QRCode.toDataURL(row.boxNumber)
      //       }));

      //       const resolvedItems = await Promise.all(templateItems);

      //       // 获取打印模板
      //       const printTemplate = this.getHiprintTemplate();

      //       // 触发打印
      //       const hiprint = new vuePluginHiprint.Hiprint();
      //       hiprint.print(printTemplate, { data: { items: resolvedItems } });
      //   },

      //   getHiprintTemplate() {
      //       // 定义打印模板
      //       return new vuePluginHiprint.HiprintTemplate()
      //           .addTable({
      //               columns: [
      //                   { field: "index", title: "编号", width: 50 },
      //                   { field: "customerName", title: "客户名称", width: 100 },
      //                   { field: "invCode", title: "存货编码", width: 100 },
      //                   { field: "invName", title: "存货名称", width: 100 },
      //                   { field: "pictureCode", title: "产品图号", width: 100 },
      //                   { field: "quantity", title: "数量", width: 50 },
      //                   { field: "boxNumber", title: "箱号", width: 100 },
      //                   { field: "productIds", title: "产品ID号", width: 150 },
      //                   { field: "createdTime", title: "包装日期", width: 100 }
      //               ],
      //               dataKey: "items",
      //               height: 1000,
      //               width: 800
      //           })
      //           .addImage({
      //               field: "qrCodeUrl",
      //               width: 100,
      //               height: 100,
      //               position: "absolute",
      //               top: 10,
      //               right: 10
      //           });
      //   }
    },
    mounted() {
      this.generateQRCode();
    },
  };
  </script>
  
  <style scoped>
  .label-container {
    max-width: 800px;
    margin: 0 auto;
    padding: 20px;
    background: #e0f0e0;
    border: 1px solid #333;
    box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
  }
  
  .label-content table {
    width: 100%;
    border-collapse: collapse;
    text-align: left;
  }
  
  .label-content td {
    border: 1px solid #000;
    padding: 5px;
  }
  
  .header {
    text-align: center;
    font-weight: bold;
    font-size: 18px;
    background: #cce5cc;
  }
  
  .qr-code img {
    display: block;
    margin: 10px auto;
    max-width: 100px;
  }
  
  .print-button {
    margin-top: 20px;
    padding: 10px 20px;
    font-size: 16px;
    background: #4caf50;
    color: #fff;
    border: none;
    cursor: pointer;
  }
  
  .print-button:hover {
    background: #45a049;
  }
  </style>
  
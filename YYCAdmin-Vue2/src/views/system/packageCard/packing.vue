<template>
    <div class="app-container">
        <el-row :gutter="20">
            <el-col :lg="20" :sm="24">
                <el-form :model="queryParams" ref="queryForm" :inline="true" label-width="68px">
                    <el-form-item label="创建时间">
                        <el-date-picker v-model="dateRange" style="width: 240px" value-format="yyyy-MM-dd"
                            type="daterange" range-separator="-" start-placeholder="开始日期"
                            end-placeholder="结束日期"></el-date-picker>
                    </el-form-item>
                    <el-form-item label="箱号" prop="boxNumber">
                        <el-input v-model="queryParams.boxNumber" placeholder="请输入箱号" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item label="产品编码" prop="invCode">
                        <el-input v-model="queryParams.invCode" placeholder="请输入产品编码" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item label="产品名称" prop="invName">
                        <el-input v-model="queryParams.invName" placeholder="请输入产品名称" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item label="零箱" prop="invCode">
                        <el-switch v-model="queryParams.isZeroBox" active-color="#13ce66" inactive-color="#ff4949">
                        </el-switch>
                    </el-form-item>
                    <el-form-item style="margin-left: 20px;">
                        <el-button size="medium" type="primary" icon="el-icon-search"
                            @click="handleQuery">搜索</el-button>
                        <el-button size="medium" icon="el-icon-refresh" @click="reset">重置</el-button>
                    </el-form-item>
                </el-form>
            </el-col>
            <el-col :lg="4" :sm="24">
                <div class="button-borders">
                    <button class="primary-button" @click="batchPrintLabels" :loading="isPrint"> 批量打印标识卡 </button>
                </div>
            </el-col>
        </el-row>
        <el-table v-loading="_loading" :data="tableData" stripe @selection-change="handleSelectionChange">
            <el-table-column type="selection"></el-table-column>
            <el-table-column type="index" label="序号"></el-table-column>
            <el-table-column prop="boxNumber" label="箱号" min-width="100"></el-table-column>
            <el-table-column prop="invCode" label="产品编码" min-width="90"></el-table-column>
            <el-table-column prop="invName" label="产品名称" min-width="180" show-overflow-tooltip></el-table-column>
            <el-table-column prop="pictureCode" label="图号" min-width="130"></el-table-column>
            <el-table-column prop="boxQty" label="装箱数量"></el-table-column>
            <el-table-column prop="quantity" label="已装数量"></el-table-column>
            <el-table-column prop="customerName" label="客户名称" min-width="190"></el-table-column>
            <el-table-column prop="createdTime" label="创建时间" min-width="140"></el-table-column>
            <el-table-column prop="createBy" label="装箱人"></el-table-column>
            <el-table-column label="操作" align="center" fixed="right" min-width="200">
                <template slot-scope="scope">
                    <el-button size="medium" @click="preview(scope.row)">预览</el-button>
                    <el-button size="medium" type="primary" @click="handledetails(scope.row)">详情</el-button>
                </template>
            </el-table-column>
        </el-table>
        <!-- 分页栏 -->
        <pagination v-show="total > 0" :page-sizes="[20, 50, 100, 500, 1000]" :total="total"
            :page.sync="queryParams.pageNum" :limit.sync="queryParams.pageSize" @pagination="getList" />
        <!-- 查看明细 -->
        <el-drawer title="详情" size="70%" :visible.sync="drawer">
            <div>
                <el-table v-loading="_loading" :data="details" stripe>
                    <el-table-column type="index" label="序号"></el-table-column>
                    <el-table-column prop="invID" label="产品ID"></el-table-column>
                    <el-table-column prop="flowCard" label="流转卡号" min-width="130"></el-table-column>
                    <el-table-column prop="invCode" label="产品编码" min-width="90"></el-table-column>
                    <el-table-column prop="invName" label="产品名称" min-width="180"></el-table-column>
                    <el-table-column prop="quantity" label="数量"></el-table-column>
                    <el-table-column prop="createdTime" label="装箱时间" min-width="140"></el-table-column>
                    <el-table-column prop="isFlag" label="是否入库">
                        <template #default="scope">
                            {{ scope.row.isFlag === 1 ? '已入库' : '未入库' }}
                        </template>
                    </el-table-column>
                    <el-table-column prop="rdCode" label="入库单号" min-width="130"></el-table-column>
                    <el-table-column prop="rdTime" label="入库时间" min-width="140"></el-table-column>
                    <el-table-column prop="rdMaker" label="入库操作人"></el-table-column>
                </el-table>
                <el-table v-loading="_loading" :data="tableData" empty-text="暂无数据" />
            </div>
        </el-drawer>

        <!-- 打印预览 -->
        <el-dialog title="打印预览" :visible.sync="isPreview" width="60%" append-to-body>
            <div class="label-container">
                <div class="label-content">
                    <!-- 表格样式 -->
                    <table>
                        <tr>
                            <td colspan="4" class="header">成品包装标识卡<br>Finished Product Packaging Label</td>
                        </tr>
                        <tr>
                            <td>编号<br>Code</td>
                            <td>{{ labelData.id }}</td>
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
                            <td>数量<br>Quantity</td>
                            <td>{{ labelData.quantity }}</td>
                            <td>箱号<br>Packing No.</td>
                            <td colspan="3">{{ labelData.packingNo }}</td>
                        </tr>
                        <tr>
                            <td>产品ID号<br>Part Serial No.</td>
                            <td colspan="3" class="part-serial-no">{{ labelData.partSerialNo }}</td>
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
                <button @click="printLabel" class="print-button">打印标识卡</button>
            </div>
        </el-dialog>
    </div>
</template>

<script>
import { packingList } from "@/api/business/packing"
import QRCode from "qrcode"

export default {
    name: 'post',
    data() {
        return {
            isPrint: false,

            // 遮罩层
            _loading: false,
            drawer: false,
            isPreview: false,
            //表数据
            tableData: [],
            details: [],
            total: 0,
            queryParams: {
                pageNum: 1,
                pageSize: 20,
                boxNumber: '',
                invCode: '',
                invName: '',
                isZeroBox: false
            },
            // 日期范围
            dateRange: [],
            selectedRows: [], // 存储选中的行数据
            //打印预览数据
            labelData: {
                id: "",
                customerName: "",
                stockCode: "",
                stockName: "",
                partNo: "",
                quantity: "",
                packingNo: "",
                partSerialNo: "",
                packingDate: "",
            },
            qrCodeUrl: "", // 二维码 URL
        }
    },
    created() {
        this.getList();
    },
    methods: {
        // 处理勾选变化
        handleSelectionChange(val) {
            this.selectedRows = val; // 更新勾选的数据
        },

        async batchPrintLabels() {
            if (this.selectedRows.length === 0) {
                this.$message.warning('请先选择要打印的标识卡');
                return;
            }

            // 等待所有行的二维码生成完成
            const printContents = await Promise.all(this.selectedRows.map(async row => {
                const labelData = {
                    id: row.id || '',
                    customerName: row.customerName || '',
                    stockCode: row.invCode || '',
                    stockName: row.invName || '',
                    partNo: row.pictureCode || '',
                    quantity: row.quantity || '',
                    packingNo: row.boxNumber || '',
                    partSerialNo: row.details ? row.details.map(detail => detail.invID).join('/') : '',
                    // partSerialNo: '250331007/250331006/250331008/250331009/250331012/250331007/250331006/250331008/250331009//250331008/250331009//250331008/250331009//250331008/250331009/',
                    packingDate: row.createdTime ? row.createdTime.split('T')[0] : '',
                };

                // 生成二维码
                let qrCodeHtml = '';
                try {
                    const url = await QRCode.toDataURL(row.barCode || ''); // 确保 barCode 存在
                    qrCodeHtml = `<img src="${url}" alt="QR Code" />`;
                } catch (err) {
                    console.error("二维码生成失败:", err);
                    qrCodeHtml = '<span>二维码生成失败</span>';
                }

                // 延迟检查内容高度（等待 DOM 渲染）
                this.$nextTick(() => {
                    const partSerialNoTd = document.querySelector('.part-serial-no');
                    if (partSerialNoTd) {
                        const tdHeight = partSerialNoTd.offsetHeight;
                        const maxHeight = 40; // 假设最大高度为 40px，需根据实际调整
                        if (tdHeight > maxHeight) {
                            partSerialNoTd.classList.add('overflow'); // 添加 overflow 类以缩小字体
                        } else {
                            partSerialNoTd.classList.remove('overflow');
                        }
                    }
                });

                // 返回单个标识卡的 HTML
                return `
            <div class="label-content">
                <table>
                    <tr>
                        <td colspan="4" class="header">成品包装标识卡<br>Finished Product Packaging Label</td>
                    </tr>
                    <tr>
                        <td>编号<br>Code</td>
                        <td>${labelData.id}</td>
                        <td>客户名称<br>Customer Name</td>
                        <td>${labelData.customerName}</td>
                    </tr>
                    <tr>
                        <td>存货编码<br>Stock Code</td>
                        <td>${labelData.stockCode}</td>
                        <td>存货名称<br>Stock Name</td>
                        <td>${labelData.stockName}</td>
                    </tr>
                    <tr>
                        <td colspan="1">产品图号<br>Part No.</td>
                        <td colspan="3">${labelData.partNo}</td>
                    </tr>
                    <tr>
                        <td>数量<br>Quantity</td>
                        <td>${labelData.quantity}</td>
                        <td>箱号<br>Packing No.</td>
                        <td colspan="3">${labelData.packingNo}</td>
                    </tr>
                    <tr>
                        <td>产品ID号<br>Part Serial No.</td>
                        <td colspan="3" class="part-serial-no">${labelData.partSerialNo}</td>
                    </tr>
                    <tr>
                        <td class="qr-code" rowspan="2" colspan="2">${qrCodeHtml}</td>
                        <td>包装日期<br>Packing Date</td>
                        <td>${labelData.packingDate}</td>
                    </tr>
                </table>
            </div>
        `;
            }));

            // 将所有标识卡内容拼接并打印（每个标识卡后添加分页）
            this.printBatch(printContents.join('<div style="page-break-after: always;"></div>'));
        },

        // 单个打印方法（复用逻辑）
        printLabel() {
            const printContent = document.querySelector('.label-content').outerHTML;
            this.printBatch(printContent);
        },

        // 通用的批量打印方法
        printBatch(content) {
            const iframe = document.createElement('iframe');
            iframe.style.position = 'absolute';
            iframe.style.width = '0';
            iframe.style.height = '0';
            iframe.style.border = 'none';
            document.body.appendChild(iframe);

            const doc = iframe.contentWindow.document;
            doc.open();
            doc.write(`
                <html>
                <head>
                    <style>
                        .label-content {
                         padding: 10rpx;
                         border-right: 1px solid #333;
                        }
                        /* 确保表格固定布局 */
                        .label-content table {
                            width: 15cm;
                            height: 10cm;
                            border-collapse: collapse;
                        }
                        .label-content td {
                            border: 1px solid #000;
                        }
                        .header {
                            text-align: center;
                            font-weight: bold;
                            background: #cce5cc;
                        }
                        .qr-code img {
                            display: block;
                            margin: 5px auto;
                        }
                        @media print {
                            body * {
                                visibility: hidden;
                            }
                            .label-content,
                            .label-content * {
                                visibility: visible;
                            }
                            .label-content {
                                position: relative;
                                width: 15cm;
                                height: 9cm;
                                padding: 0;
                                margin: 0 auto;
                                box-sizing: border-box;
                                font-size: 10px; /* 减小字体大小以适应新高度 */
                                page-break-inside: avoid;
                            }
                            .label-content table {
                                width: 15cm;
                                height: 9cm;
                                border-collapse: collapse;
                            }
                            .label-content td {
                                font-size: 15px;
                                padding: 4px; /* 减小单元格内边距 */
                                line-height: 1; /* 减小行高 */
                                border: 0.5px solid #000; /* 减小边框厚度 */
                            }
                            .header {
                                font-size: 20px;/* 减小标题字体大小 */
                                line-height: 1;
                                padding: 4px;
                            }
                            .qr-code img {
                                max-width: 80px; /* 减小二维码尺寸以适应新高度 */
                                max-height: 80px;
                                margin: 0.5px auto;
                            }
                            .part-serial-no {
                                white-space: normal; /* 允许换行 */
                                word-break: break-all; /* 强制所有内容换行 */
                                word-wrap: break-word; /* 长内容自动换行 */
                                font-size: 8px; !important /* 默认字体大小 */
                                overflow: hidden;
                            }
                        }
                    </style>
                </head>
                <body>${content}</body>
                </html>
            `);
            doc.close();

            setTimeout(() => {
                iframe.contentWindow.focus();
                iframe.contentWindow.print();
                setTimeout(() => {
                    document.body.removeChild(iframe);
                }, 1000);
            }, 500); // 延迟 500ms 确保内容渲染完成
        },

        //打印预览
        preview(row) {
            this.labelData.id = row.id;
            this.labelData.customerName = row.customerName;
            this.labelData.stockCode = row.invCode;
            this.labelData.stockName = row.invName
            this.labelData.partNo = row.pictureCode;
            this.labelData.quantity = row.quantity;
            this.labelData.packingNo = row.boxNumber;
            // 将拼接后的字符串赋值给 labelData.partSerialNo
            this.labelData.partSerialNo = row.details.map(detail => detail.invID).join('/');
            //截取年月日
            this.labelData.packingDate = row.createdTime.split('T')[0];
            //生成二维码
            QRCode.toDataURL(row.barCode)
                .then((url) => {
                    this.qrCodeUrl = url;
                })
                .catch((err) => {
                    this.msgError("二维码生成失败");
                    console.error("二维码生成失败:", err);
                });

            this.isPreview = true;
        },
        /** 获取列表 */
        getList() {
            this._loading = true
            packingList(this.addDateRange(this.queryParams, this.dateRange)).then(response => {
                console.log(JSON.stringify(response.data.result));
                this.tableData = response.data.result
                this.total = response.data.totalNum
                this._loading = false

                if (this.tableData.length === 0) {
                    this.msgWarning('未查询到装箱标识卡数据')
                }
            })
        },
        /** 搜索按钮操作 */
        handleQuery() {
            this.queryParams.page = 1
            this.getList()
        },
        /** 查看明细 */
        handledetails(row) {
            this.drawer = true;
            this.details = row.details;
        },
        reset() {
            this.dateRange = [];
            this.queryParams.boxNumber = '';
            this.queryParams.invCode = '';
            this.queryParams.invName = '';
            this.queryParams.isZeroBox = false;
        }


    }
}
</script>

<style scoped>
/* From Uiverse.io by faizanullah1999 */
.primary-button {
    font-family: 'Ropa Sans', sans-serif;
    /* font-family: 'Valorant', sans-serif; */
    color: white;
    cursor: pointer;
    font-size: 13px;
    font-weight: bold;
    letter-spacing: 0.05rem;
    border: 1px solid #F5222D;
    padding: 0.8rem 2.1rem;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 531.28 200'%3E%3Cdefs%3E%3Cstyle%3E .shape %7B fill: %231890FF; %7D %3C/style%3E%3C/defs%3E%3Cg id='Layer_2' data-name='Layer 2'%3E%3Cg id='Layer_1-2' data-name='Layer 1'%3E%3Cpolygon class='shape' points='415.81 200 0 200 115.47 0 531.28 0 415.81 200' /%3E%3C/g%3E%3C/g%3E%3C/svg%3E%0A");
    background-color: #F5222D;
    background-size: 200%;
    background-position: 200%;
    background-repeat: no-repeat;
    transition: 0.3s ease-in-out;
    transition-property: background-position, border, color;
    position: relative;
    z-index: 1;
}

.primary-button:hover {
    border: 1px solid #1890FF;
    color: white;
    background-position: 40%;
}

.primary-button:before {
    content: "";
    position: absolute;
    background-color: #F5222D;
    width: 0.2rem;
    height: 0.2rem;
    top: -1px;
    left: -1px;
    transition: background-color 0.15s ease-in-out;
}

.primary-button:hover:before {
    background-color: white;
}

.primary-button:hover:after {
    background-color: white;
}

.primary-button:after {
    content: "";
    position: absolute;
    background-color: #1890FF;
    width: 0.3rem;
    height: 0.3rem;
    bottom: -1px;
    right: -1px;
    transition: background-color 0.15s ease-in-out;
}

.button-borders {
    position: relative;
    width: fit-content;
    height: fit-content;
}

.button-borders:before {
    content: "";
    position: absolute;
    width: calc(100% + 0.5em);
    height: 50%;
    left: -0.3em;
    top: -0.3em;
    border: 1px solid #F5222D;
    border-bottom: 0px;
    /* opacity: 0.3; */
}

.button-borders:after {
    content: "";
    position: absolute;
    width: calc(100% + 0.5em);
    height: 50%;
    left: -0.3em;
    bottom: -0.3em;
    border: 1px solid #F5222D;
    border-top: 0px;
    /* opacity: 0.3; */
    z-index: 0;
}

.shape {
    fill: #F5222D;
}
</style>

<style>
/* 现有非打印样式保持不变 */
.label-container {
    width: 100%;
    padding: 20px;
}

.label-content {
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
    margin: 5px auto;
    max-width: 100px;
}

.part-serial-no {
    white-space: normal;
    /* 允许换行 */
    word-break: break-all;
    /* 强制所有内容换行 */
    word-wrap: break-word;
    /* 长内容自动换行 */
    font-size: 8px;
    /* 默认字体大小 */
    overflow: hidden;
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
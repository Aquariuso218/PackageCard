<template>
    <div class="app-container">
        <el-row :gutter="20">
            <el-col :lg="20" :sm="24">
                <el-form :model="queryParams" ref="queryForm" :inline="true" label-width="68px">
                    <el-form-item label="箱号" prop="boxNumber">
                        <el-input v-model="queryParams.boxNumber" placeholder="请输入箱号" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item label="产品编码" prop="invCode">
                        <el-input v-model="queryParams.invCode" placeholder="请输入产品编码" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item>
                        <el-button type="primary" icon="el-icon-search" @click="handleQuery">搜索</el-button>
                        <el-button icon="el-icon-refresh" @click="">重置</el-button>
                    </el-form-item>
                </el-form>
            </el-col>
            <el-col :lg="4" :sm="24">
                <div class="button-borders">
                    <button class="primary-button" @click=""> 打印标识卡 </button>
                </div>
            </el-col>
        </el-row>

        <el-table v-loading="loading" :data="tableData">
            <el-table-column type="index" label="序号"></el-table-column>
            <el-table-column prop="boxNumber" label="箱号"></el-table-column>
            <el-table-column prop="invCode" label="产品编码"></el-table-column>
            <el-table-column prop="invName" label="产品名称"></el-table-column>
            <el-table-column prop="pictureCode" label="图号"></el-table-column>
            <el-table-column prop="quantity" label="数量"></el-table-column>
            <el-table-column prop="customerName" label="客户名称"></el-table-column>
            <el-table-column prop="printCount" label="是否打印"></el-table-column>
            <el-table-column prop="createdTime" label="创建时间"></el-table-column>

            <el-table-column label="操作" align="center" fixed="right" width="160">
                <template slot-scope="scope">
                    <el-button size="medium" @click="handledetails(scope.row)">查看</el-button>
                </template>
            </el-table-column>
        </el-table>

        <pagination v-show="total > 0" :page-sizes="[50, 100, 500, 1000]" :total="total"
            :page.sync="queryParams.pageNum" :limit.sync="queryParams.pageSize" @pagination="getList" />

        <!-- 查看明细 -->
        <el-drawer title="详情" size="70%" :visible.sync="drawer">
            <div>
                <el-table v-loading="loading" :data="details">
                    <el-table-column type="index" label="序号"></el-table-column>
                    <el-table-column prop="flowCard" label="流转卡号"></el-table-column>
                    <el-table-column prop="quantity" label="数量"></el-table-column>
                    <el-table-column prop="createdTime" label="创建时间"></el-table-column>
                </el-table>

                <el-table v-loading="loading" :data="tableData" empty-text="暂无数据" />
            </div>
        </el-drawer>
    </div>
</template>

<script>
import { packingList } from "@/api/business/packing"
import QRCode from "qrcode"

export default {
    name: 'post',
    data() {
        return {
            // 遮罩层
            loading: false,
            drawer: false,
            //表数据
            tableData: [],
            details: [],
            total: 0,
            queryParams: {
                pageNum: 1,
                pageSize: 50,
                boxNumber: '',
                invCode: ''
            }
        }
    },
    created() {
    },
    methods: {
        /** 获取列表 */
        getList() {
            this.loading = true
            packingList(this.queryParams).then(response => {
                this.tableData = response.data.result
                this.total = response.data.totalNum
                this.loading = false
            })
        },
        /** 搜索按钮操作 */
        handleQuery() {
            if (!this.queryParams.boxNumber && !this.queryParams.invCode) {
                this.msgError('请输入箱号或产品编码');
                return;
            }
            this.queryParams.page = 1
            this.getList()
        },
        /** 查看明细 */
        handledetails(row) {
            this.drawer = true
            this.details = row.details;
        },

        
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
/********************打印模板样式 ********************************/
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
<template>
    <div class="app-container">
        <el-row :gutter="20" class="mb8">
            <el-col :lg="20" :sm="24">
                <el-form :model="queryParams" ref="queryForm" :inline="true" label-width="68px">
                    <el-form-item label="创建时间">
                        <el-date-picker v-model="dateRange" style="width: 240px" value-format="yyyy-MM-dd"
                            type="daterange" range-separator="-" start-placeholder="开始日期"
                            end-placeholder="结束日期"></el-date-picker>
                    </el-form-item>
                    <el-form-item label="产品编码" prop="boxNumber">
                        <el-input v-model="queryParams.invCode" placeholder="请输入产品编码" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
                    </el-form-item>
                    <el-form-item label="产品名称" prop="invName">
                        <el-input v-model="queryParams.invName" placeholder="请输入产品名称" clearable style="width: 240px"
                            @keyup.enter.native="handleQuery" />
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
                    <button class="primary-button" @click="create" :loading="isCreating"> 特殊装箱 </button>
                </div>
            </el-col>
        </el-row>

        <el-table v-loading="_loading" :data="tableData" ref="tableRef" @selection-change="handleSelectionChange">
            <el-table-column type="selection"></el-table-column>
            <el-table-column type="index" label="序号"></el-table-column>
            <el-table-column prop="flowCard" label="流转卡" min-width="120"></el-table-column>
            <el-table-column prop="id" label="ID"></el-table-column>
            <el-table-column prop="invCode" label="产品编码"></el-table-column>
            <el-table-column prop="invName" label="产品名称" min-width="180" show-overflow-tooltip></el-table-column>
            <el-table-column prop="cInvStd" label="产品规格" min-width="180" show-overflow-tooltip></el-table-column>
            <el-table-column prop="quantity" label="数量"></el-table-column>
            <el-table-column prop="boxNumber" label="箱码" min-width="160">
                <template slot-scope="scope">
                    <el-input v-model="scope.row.boxNumber" placeholder="请输入箱码"></el-input>
                </template>
            </el-table-column>
            <el-table-column prop="docDate" label="日期" min-width="140"></el-table-column>
            <el-table-column prop="customerName" label="客户名称" min-width="160"></el-table-column>
            <el-table-column prop="pictureCode" label="图号" min-width="130"></el-table-column>
        </el-table>

        <pagination v-show="total > 0" :page-sizes="[20, 50, 100, 500, 1000]" :total="total"
            :page.sync="queryParams.pageNum" :limit.sync="queryParams.pageSize" @pagination="getList" />
    </div>
</template>


<script>
import { invList, specialCreatePacking } from "@/api/business/packing"
import { getUserProfile } from '@/api/system/user'
export default {
    name: 'post',
    data() {
        return {
            userInfo: {},
            isCreating: false, // 控制普通装箱按钮

            // 遮罩层
            _loading: true,
            isPacking: false,

            selectedRows: [], // 存储勾选的行数据

            //表数据
            tableData: [],
            total: 0,
            queryParams: {
                pageNum: 1,
                pageSize: 20,
                invCode: '',
                invName: '',
                mergeBoxCode: '',
                cInvcCode: '' //筛选某个特定产品时使用，通过存货父级判断
            },
            // 日期范围
            dateRange: [],

        }
    },
    created() {
        getUserProfile().then((response) => {
            this.userInfo = response.data.user
        })
        this.getList()
    },
    methods: {
        // 处理勾选变化
        handleSelectionChange(val) {
            this.selectedRows = val; // 更新勾选的数据
        },
        async getList() {
            const loading = this.showLoading('查询中...');
            this._loading = true;

            try {
                await invList(this.addDateRange(this.queryParams, this.dateRange)).then(response => {
                    this.tableData = response.data.result
                    this.total = response.data.totalNum
                    this._loading = false;
                    loading.close();

                    if (this.tableData.length === 0) {
                        this.msgWarning('未查询到产品数据')
                    }
                })
            } finally {
                loading.close();
            }
        },
        create() {
            if (this.isCreating) return; // 防止重复点击
            this.isCreating = true; // 禁用按钮
            const loading = this.showLoading('装箱中...');
            specialCreatePacking({
                simulatedDatas: this.selectedRows,
                createName: this.userInfo.nickName
            }).then(response => {
                if (response.data.includes('成功')) {
                    this.msgSuccess('装箱成功')
                    this.queryParams.page = 1
                    this.getList()
                } else {
                    this.msgWarning(response.data)
                }
            })
            loading.close();
            this.isCreating = false; // 恢复按钮
        },
        /** 搜索按钮操作 */
        handleQuery() {
            this.queryParams.page = 1
            this.getList()
        },
        reset() {
            this.dateRange = [];
            this.queryParams.invCode = '';
            this.queryParams.invName = '';
            this.queryParams.mergeBoxCode = '';
        }
    },
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
    border: 1px solid #45a247;
    padding: 0.8rem 2.1rem;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 531.28 200'%3E%3Cdefs%3E%3Cstyle%3E .shape %7B fill: %231890FF; %7D %3C/style%3E%3C/defs%3E%3Cg id='Layer_2' data-name='Layer 2'%3E%3Cg id='Layer_1-2' data-name='Layer 1'%3E%3Cpolygon class='shape' points='415.81 200 0 200 115.47 0 531.28 0 415.81 200' /%3E%3C/g%3E%3C/g%3E%3C/svg%3E%0A");
    background-color: #45a247;
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
    background-color: #45a247;
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
    border: 1px solid #45a247;
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
    border: 1px solid #45a247;
    border-top: 0px;
    /* opacity: 0.3; */
    z-index: 0;
}

.shape {
    fill: #45a247;
}
</style>
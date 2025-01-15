<template>
  <div class="app-container">
    <el-row :gutter="20" >
      <!-- 通过offset将元素移动至相应位置 -->
      <el-col :lg="4" :sm="24" :offset="20">
        <div class="button-borders">
          <button class="primary-button" @click="create"> 生成标识卡 </button>
        </div>
      </el-col>
    </el-row>

    <el-table v-loading="loading" :data="tableData">
      <el-table-column type="index"></el-table-column>
      <el-table-column prop="flowCard" label="流转卡"></el-table-column>
      <el-table-column prop="invCode" label="产品"></el-table-column>
      <el-table-column prop="quantity" label="数量"></el-table-column>
    </el-table>

    <pagination v-show="total > 0" :page-sizes="[50,100,500,1000]" :total="total" :page.sync="queryParams.pageNum" :limit.sync="queryParams.pageSize"
      @pagination="getList" />
  </div>
</template>


<script>
import { invList,createPacking } from "@/api/business/packing"

export default {
  name: 'post',
  data() {
    return {
      // 遮罩层
      loading: true,
      //表数据
      tableData: [],
      total: 0,
      queryParams: {
        pageNum: 1,
        pageSize: 50,
      }

    }
  },
  created() {
    this.getList()
  },
  methods: {
    getList() {
      this.loading = true
      invList(this.queryParams).then(response => {
        this.tableData = response.data.result
        this.total = response.data.totalNum
        this.loading = false
      })
    },
    create(){
      createPacking(this.tableData).then(response => {
        if(response.data = '生成成功'){
          this.msgSuccess('生成成功')
        }else{
          this.msgInfo(response.data)
        }
      })
    },
    /** 搜索按钮操作 */
    handleQuery() {
      this.queryParams.page = 1
      this.getList()
    },
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

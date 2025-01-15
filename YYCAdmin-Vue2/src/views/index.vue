<template>
  <div class="app-container home">

    <el-row :gutter="20">
      <el-col :sm="18" :lg="24">
        <blockquote class="text-warning" style="font-size: 14px">
          <h2>云优创后台管理系统</h2>
          <p>云优创后台管理系统为企业提供更多定制化开发解决方案。</p>
        <p>
          <b>当前版本:</b> <span>v{{ version }}</span>
        </p>
        <p>
          <el-button type="primary" size="mini" icon="el-icon-cloudy" plain @click="goTarget('http://www.yonyouc.com')"
            >访问官网
          </el-button>
          <el-button type="primary" size="mini" icon="el-icon-cloudy" plain @click="goTarget('https://www.yonyou.com/')"
            >用友官网
          </el-button>
        </p>
        <p></p>
        <h3>⭐ 您的需求，是我们不变的关注点；您的满意，是我们不断追求的目标。⭐</h3>
        </blockquote>
      </el-col>
    </el-row> 
    <el-divider />
    <el-row :gutter="20" class="mt10">
      <el-col :sm="24" :lg="6">
        <el-card>
          <div slot="header" class="clearfix">
            <span>联系信息</span>
          </div>
          <div class="body">
            <p>
              <i class="el-icon-s-promotion"></i>销售专线：
              <a>023-67877790</a>
            </p>
          </div>
          <div class="body">
            <p>
              <i class="el-icon-user-solid"></i>服务专线：
              <a>023-67877790</a>
            </p>
          </div>
        </el-card>
      </el-col>
      <el-col :sm="24" :lg="8">
      </el-col>
      <el-col :sm="24" :lg="10">
      </el-col>
    </el-row>
    <el-divider />
  </div>
</template>

<script>
const defaultSettings = require('@/settings.js')
export default {
  name: 'index',
  data() {
    return {
      // 版本号
      version: defaultSettings.version,
    }
  },
  mounted() {
    // this.$notify({
    //   title: 'Vue2停止更新公告',
    //   message:
    //     'Vue2已不再更新新功能，请使用Vue3版本<a href="https://gitee.com/izory/ZRAdmin-vue" target="_blank"> https://gitee.com/izory/ZRAdmin-vue </a>',
    //   position: 'bottom-right',
    //   type: 'warning',
    //   dangerouslyUseHTMLString: true,
    //   duration: 0,
    // })
  },
  methods: {
    goTarget(href) {
      window.open(href, '_blank')
    },
  },
}
</script>

<style scoped lang="scss">
.tip {
  color: red;
  display: inline-block;
  height: 30px;
  line-height: 30px;
}
.home {
  blockquote {
    padding: 10px 20px;
    margin: 0 0 20px;
    font-size: 17.5px;
    border-left: 5px solid #eee;
  }
  hr {
    margin-top: 20px;
    margin-bottom: 20px;
    border: 0;
    border-top: 1px solid #eee;
  }
  .col-item {
    margin-bottom: 20px;
  }

  ul {
    padding: 0;
    margin: 0;
  }

  font-family: 'open sans', 'Helvetica Neue', Helvetica, Arial, sans-serif;
  font-size: 13px;
  color: #676a6c;
  overflow-x: hidden;

  ul {
    list-style-type: none;
  }

  h4 {
    margin-top: 0px;
  }

  h2 {
    margin-top: 10px;
    font-size: 26px;
    font-weight: 100;
  }

  p {
    margin-top: 10px;

    b {
      font-weight: 700;
    }
  }

  .update-log {
    ol {
      display: block;
      list-style-type: decimal;
      margin-block-start: 1em;
      margin-block-end: 1em;
      margin-inline-start: 0;
      margin-inline-end: 0;
      padding-inline-start: 40px;
    }
  }
}
</style>

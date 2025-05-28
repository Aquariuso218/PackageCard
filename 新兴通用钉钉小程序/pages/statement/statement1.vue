<template>
	<view style="padding: 0 5rpx;">
		<!-- 说明 -->
		<view class="msgView">
			<view class="msg ">单元格说明:</view>
			<view class="msg txt">差材料</view>
			<view class="msg bg-red">超期/未送货</view>
			<view class="msg bg-yellow">超期/已送货</view>
			<view class="msg bg-green">未超期/已送货</view>
		</view>
		<!-- 表格 -->
		<view class="uni-container" v-if="this.tableData.length !== 0">
			<uni-table ref="tableData" border stripe>
				<view class="tableHead">
					<uni-tr>
						<uni-th width="30" align="center">N.</uni-th>
						<uni-th width="110" align="center">存货名称</uni-th>
						<uni-th width="110" align="center">规格型号</uni-th>
						<uni-th width="70" align="center">工序</uni-th>
						<uni-th width="65" align="center">总计划</uni-th>
						<uni-th width="70" align="center">材料结存</uni-th>
						<uni-th width="70" align="center">材料差缺</uni-th>
						<uni-th width="70" align="center">前期差缺</uni-th>
						<uni-th v-for="(date, index) in dynamicDates" :key="index" width="60" align="center">
							{{ date }}
						</uni-th>
						<uni-th width="70" align="center">后期计划</uni-th>
						<uni-th width="70" align="center">来料在途</uni-th>
						<uni-th width="70" align="center">送货在途</uni-th>
					</uni-tr>
				</view>
				<view class="tableBody">
					<uni-tr v-for="(item, rowIndex) in tableData" :key="rowIndex">
						<uni-td width="30" align="center">{{ item.cXh }}</uni-td>
						<uni-td width="110" @click="showPopup('存货名称', item.cInvName)">
							{{ item.cInvName.length > 6 ? item.cInvName.slice(0, 6) + '...' : item.cInvName }}
						</uni-td>
						<uni-td width="110" @click="showPopup('规格型号', item.cInvStd)">
							{{ item.cInvStd.length > 8 ? item.cInvStd.slice(0, 8) + '...' : item.cInvStd }}
						</uni-td>
						<uni-td width="70" @click="showPopup('工序', item.Description)">
							{{ item.Description.length > 4 ? item.Description.slice(0, 4) + '...' : item.Description }}
						</uni-td>
						<uni-td width="65" align="right">{{ item.cTotalJHQty || '' }}</uni-td>
						<uni-td width="70" align="right">{{ item.cZCLJCQty || '' }}</uni-td>
						<uni-td width="70" align="right">{{ item.cZCLCQQty || '' }}</uni-td>
						<uni-td width="70" align="right">
							<view class="td-full"
								:class="{ 'bg-green': item.cQQState === 1, 'bg-yellow': item.cQQState === 3, 'bg-red': item.cQQState === 4, 'txt': item.cQQZTState === 1 }">
								{{ item.cQQQty || '' }}
							</view>
						</uni-td>
						<uni-td width="60" v-for="(date, idx) in dynamicDates" :key="idx" align="right">
							<view :class="{
								'bg-green': item[`CQState${idx + 1}`] === 1,
								'bg-yellow': item[`CQState${idx + 1}`] === 3,
								'bg-red': item[`CQState${idx + 1}`] === 4,
								'txt': item[`cZTState${idx + 1}`] === 1
							}">
								{{ item[`D${idx + 1}`] || '' }}
							</view>
						</uni-td>
						<uni-td width="70" align="right"
							:class="{ 'bg-green': item.cHQState === 1, 'bg-yellow': item.cHQState === 3, 'bg-red': item.cHQState === 4, 'txt': item.cHQState === 1 }">{{
								item.cHQQty || '' }}</uni-td>
						<uni-td width="70" align="right">{{ item.cLLZtQty || '' }}</uni-td>
						<uni-td width="70" align="right">{{ item.cSHZtQty || '' }}</uni-td>
					</uni-tr>
				</view>
			</uni-table>
			<!-- 加载状态提示 -->
			<!-- <view class="load-more">
				<text v-if="loading">加载中...</text>
				<text v-else-if="noMoreData">没有更多数据了</text>
				<text v-else>上拉加载更多</text>
			</view> -->
		</view>
		<!-- 数据为空 -->
		<view v-else="this.tableData.length == 0 && this.loding == false">
			<u-empty :mode="'data'" :icon="'https://cdn.uviewui.com/uview/demo/empty/data.png'">
			</u-empty>
		</view>
		<!-- 加载动画 -->
		<u-overlay :show="loding">
			<view class="warp">
				<view class="rect">
					<div class="dot-spinner">
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
						<div class="dot-spinner__dot"></div>
					</div>
				</view>
			</view>
		</u-overlay>
		<!-- 悬浮按钮1（查找） -->
		<view class="bg-white fixed-bottom-right">
			<u-button type="primary" @click="searchOpen = true" text="查询">
			</u-button>
		</view>
		<!-- 悬浮按钮2（加载更多） -->
		<view class="bg-white fixed-bottom-left">
			<u-button type="success" :plain="true" size="small" @click="loadingMore" text=">更多">
			</u-button>
		</view>

		<!-- 报表条件筛选窗口 -->
		<u-popup :customStyle="popupStyle" :show="searchOpen" ss closeable="true" mode="center" :round="10"
			@close="searchClose">
			<view class="drawer">
				<!-- <view class="searchTitle">
					筛选条件
				</view> -->
				<uni-row class="uni-row">
					<uni-col :span="2">
						<view class="uni-col-view">日期:</view>
					</uni-col>
					<uni-col :span="8">
						<view class="uni-col-view" @click="startShow = true"
							style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{ startTime }}</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="2">
						<view class="uni-col-view">到:</view>
					</uni-col>
					<uni-col :span="8">
						<view class="uni-col-view" @click="endShow = true"
							style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{ endTime }}</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<u-datetime-picker ref="datetimePicker" :formatter="formatter" @confirm="startConfirm"
						@cancel="startCancel" :show="startShow" v-model="startTimeCode" mode="date"></u-datetime-picker>
					<u-datetime-picker ref="datetimePicker" :formatter="formatter" @confirm="endConfirm"
						@cancel="endCancel" :show="endShow" v-model="endTimeCode" mode="date"></u-datetime-picker>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="2">
						<view class="uni-col-view">供应商:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<!-- <u--input placeholder="请输入内容" border="surround" clearable v-model="cVenCode"
							value=""></u--input> -->
						<view class="uni-col-view" @click="drawerOpen1"
							style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{
								selectedItem1.cVenName || '请点击选择'
							}} </view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="2">
						<view class="uni-col-view">物料:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<!-- <u--input placeholder="请输入内容" border="surround" clearable v-model="cInvCode"
							value=""></u--input> -->
						<view class="uni-col-view" @click="drawerOpen"
							style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{ selectedItem.cInvName
								|| '请点击选择'
							}} </view>
					</uni-col>
					<uni-col :span="2"></uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="2">
						<view class="uni-col-view">材料:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<u--input placeholder="请输入内容" border="surround" clearable v-model="cInvDefine2"
							value=""></u--input>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="2">
						<view class="uni-col-view">生产线:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<u--input placeholder="请输入内容" border="surround" clearable v-model="cInvscx"></u--input>
					</uni-col>
					<uni-col :span="2"></uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="2">
						<view class="uni-col-view">配套员:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<view class="uni-col-view" @click="drawerOpen2"
							style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{
								selectedItem2.cPersonName
								|| '请点击选择'
							}} </view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="2">
						<view class="uni-col-view">关键字:</view>
					</uni-col>
					<uni-col :span="8" class="divRight">
						<u--input placeholder="请输入内容" border="surround" adjust-position="true" clearable
							v-model="searchvalue"></u--input>
					</uni-col>
					<uni-col :span="2"></uni-col>
				</uni-row>

				<uni-row>
					<uni-col :span="5" class="divRight">
						<view class="uni-col-view">只看发起送货:
							<switch :checked="isSHState !== '0'" @change="toggleSHState" color="#FFCC33"
								style="transform:scale(0.7)" />
						</view>
					</uni-col>
					<uni-col :span="5" class="divRight">
						<view class="uni-col-view">只看材料差缺:
							<switch :checked="isCQState !== '0'" @change="toggleCQState" color="#FFCC33"
								style="transform:scale(0.7)" />
						</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="5" class="divRight">
						<view class="uni-col-view">只看来料转移:
							<switch :checked="isLLState !== '0'" @change="toggleLLState" color="#FFCC33"
								style="transform:scale(0.7)" />
						</view>
					</uni-col>
					<uni-col :span="5" class="divRight">
						<view class="uni-col-view">只看现在超期:
							<switch :checked="isXCState !== '0'" @change="toggleXCState" color="#FFCC33"
								style="transform:scale(0.7)" />
						</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="10">
						<view style="display: flex; justify-content: space-between; align-items: center; width: 80%;">
							<view style="width: 30%;color: white;"></view>
							<u-button text="重置" color="#bdc3c7" @click="rest"></u-button>
							<view style="width: 30%;color: white;"></view>
						</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
					<uni-col :span="10">
						<view style="display: flex; justify-content: space-between; align-items: center; width: 80%;">
							<view style="width: 30%;color: white;"></view>
							<u-button type="primary" text="查询" @click="search"></u-button>
							<view style="width: 30%;color: white;"></view>
						</view>
					</uni-col>
					<uni-col :span="2"></uni-col>
				</uni-row>
			</view>
		</u-popup>
		<!-- 产品选择器 -->
		<u-popup :customStyle="select_popupStyle" overlay="false" :show="drawerRight" closeable="true"
			@close="drawerClose" mode="right" :round="10">
			<view class="_container">
				<!-- 搜索区域 -->
				<view class="search-header">
					<view class="search-box">
						<view style="width: 80%;">
							<u--input placeholder="请输入物料编码或名称" border="surround" clearable v-model="searchInput"
								suffixIcon="search" iconStyle="color: #909399;" class="search-input"
								@input="getInvList"></u--input>
						</view>
						<view style="width: 20%;">
							<u-button text="重置" @click="restSelectItem" class="action-btn"></u-button>
						</view>
					</view>
				</view>

				<!-- 查询结果显示区域 -->
				<view class="result-list" v-if="invList.length">
					<view v-for="(item, index) in invList" :key="index"
						:class="['result-item', { 'selected': selectedItem.cInvCode === item.cInvCode }]"
						@click="selectItem(item)">
						<text>{{ item.cInvCode }} - {{ item.cInvName }}</text>
					</view>
				</view>
				<view v-else class="no-data">
					<text>没有找到数据</text>
				</view>
			</view>
		</u-popup>
		<!-- 供应商选择器 -->
		<u-popup :customStyle="select_popupStyle" overlay="false" :show="drawerRight1" closeable="true"
			@close="drawerClose1" mode="right" :round="10">
			<view class="_container">
				<!-- 搜索区域 -->
				<view class="search-header">
					<view class="search-box">
						<view style="width: 80%;">
							<u--input placeholder="请输入供应商编码或名称" border="surround" clearable v-model="searchInput1"
								suffixIcon="search" iconStyle="color: #909399;" class="search-input"
								@input="getVenList"></u--input>
						</view>
						<view style="width: 20%;">
							<u-button text="重置" @click="restSelectItem1" class="action-btn"></u-button>
						</view>
					</view>
				</view>

				<!-- 查询结果显示区域 -->
				<view class="result-list" v-if="venList.length">
					<view v-for="(item, index) in venList" :key="index"
						:class="['result-item', { 'selected': selectedItem1.cVenCode === item.cVenCode }]"
						@click="selectItem1(item)">
						<text>{{ item.cVenCode }} - {{ item.cVenName }}</text>
					</view>
				</view>
				<view v-else class="no-data">
					<text>没有找到数据</text>
				</view>
			</view>
		</u-popup>
		<!-- 配套员选择器 -->
		<u-popup :customStyle="select_popupStyle" overlay="false" :show="drawerRight2" closeable="true"
			@close="drawerClose2" mode="right" :round="10">
			<view class="_container">
				<view style="width: 20%;">
					<u-button text="重置" @click="restSelectItem2" class="action-btn"></u-button>
				</view>
				<!-- 查询结果显示区域 -->
				<view class="result-list" style="margin-top: 5rpx;" v-if="personList.length">
					<view v-for="(item, index) in personList" :key="index"
						:class="['result-item', { 'selected': selectedItem2.cPersonCode === item.cPersonCode }]"
						@click="selectItem2(item)">
						<text>{{ item.cPersonCode }} - {{ item.cPersonName }}</text>
					</view>
				</view>
				<view v-else class="no-data">
					<text>没有找到数据</text>
				</view>
			</view>
		</u-popup>
		<!-- 单元格详情弹窗 -->
		<u-popup :customStyle="_popupStyle" overlay="false" :show="popupVisible" closeable="true" @close="closePopup"
			mode="center" :round="10">
			<view class="popup">
				<view class="popup-content">
					<view class="popup-header">
						<text>{{ popupTitle + ':' }}</text>
					</view>
					<view class="popup-body">
						<text>{{ popupContent }}</text>
					</view>
				</view>
			</view>
		</u-popup>

	</view>
</template>

<script>
import { GetOrderByHY, invList, venList, personList } from "@/nxTemp/apis/sunset.js"
export default {
	data() {
		return {
			//筛选框样式
			popupStyle: {
				width: '80vw',
				height: '80vh',
				backgroundColor: '#ffffff', // 添加背景色
				// borderRadius: '20rpx', // 添加圆角
			},
			_popupStyle: {
				width: '300rpx',
				height: '15vh',
				backgroundColor: '#ffffff', // 添加背景色
				// borderRadius: '20rpx', // 添加圆角
			},
			select_popupStyle: {
				width: '50vw',
				height: '100vh',
				backgroundColor: '#ffffff', // 添加背景色
				// borderRadius: '20rpx', // 添加圆角
			},

			tableData: [],
			userInfo: {},
			dynamicDates: [], //动态数据
			invList: [],
			venList: [],
			personList: [],

			searchInput: '', //物料查找关键字
			selectedItem: null,  // 存储选中的产品数据
			searchInput1: '', //供应商查找关键字
			selectedItem1: null,  // 存储选中的供应商数据
			searchInput2: '', //配套员查找关键字
			selectedItem2: null,  // 存储选中的配套员数据

			scrollTop: 0, //屏幕位置表示，用于屏幕向上滚动

			//查询条件
			currentPage: 1,
			pageSize: 40,
			startTime: "",
			endTime: "",
			cVenCode: "",
			cInvCode: "",
			searchvalue: "", //模糊查询值
			cInvDefine2: "", //材料
			cInvscx: "", 	 //产线
			cPersonCode: "", //配套员
			isSHState: "0",  //是否只看送货
			isCQState: "0",  //是否只看查缺
			isLLState: "0", //只看来料转移
			isXCState: "0", //只看已超期

			startTimeCode: Number(new Date()),
			endTimeCode: Number(new Date()),

			searchOpen: false,
			loading: false,
			noMoreData: false,
			loding: false,
			startShow: false,
			endShow: false,
			drawerRight: false,	//查询物料
			drawerRight1: false,	//查询供应商
			drawerRight2: false,	//查询配套员

			popupVisible: false, // 控制浮动窗的显示与隐藏
			popupTitle: '',      // 浮动窗的标题
			popupContent: '',     // 浮动窗的内容

			inputBoxStyle: 'bottom:0px',  // 用来控制输入框的位置
			scrollViewHeight: '100vh',  // scroll-view 的高度
		};
	},
	onReady() {

	},
	onLoad() {
		uni.getStorage({
			key: 'userData',
			success: (res) => {
				this.userInfo = res.data; // 将用户数据赋值到当前页面的数据对象
			},
			fail: () => {
				uni.showToast({
					title: '用户数据读取失败，请重新登录',
					icon: 'none'
				});
			}
		});

		setTimeout(() => {
			this.getDefaultDateRange();
			this.search();
			this.getInvList();
			this.getVenList();
			this.getPersonList();
		}, 500);

	},
	mounted() {
	},
	methods: {

		// 选中物料项
		selectItem(item) {
			this.selectedItem = this.selectedItem === item ? null : item; // 切换选中状态
			if (this.selectedItem !== null) {
				this.cInvCode = item.cInvCode; // 更新 cInvCode
			} else {
				this.cInvCode = ''; // 清空 cInvCode
			}
			this.drawerRight = false; // 假设这是其他操作
		},
		drawerOpen() {
			this.drawerRight = true;
			console.log(this.drawerRight)
		},
		drawerClose() {
			this.drawerRight = false;
		},
		restSelectItem() {
			this.selectedItem = null;
			this.cInvCode = ''; // 清空 cInvCode
		},

		// 选中供应商项
		selectItem1(item) {
			this.selectedItem1 = this.selectedItem1 === item ? null : item; // 切换选中状态
			if (this.selectedItem1 !== null) {
				this.cVenCode = item.cVenCode; // 更新 cInvCode
			} else {
				this.cVenCode = ''; // 清空 cInvCode
			}
			this.drawerRight1 = false; // 假设这是其他操作
		},
		drawerOpen1() {
			this.drawerRight1 = true;
		},
		drawerClose1() {
			this.drawerRight1 = false;
		},
		restSelectItem1() {
			this.selectedItem1 = null;
			this.cVenCode = ''; // 清空
		},
		// 选中采购员
		selectItem2(item) {
			this.selectedItem2 = this.selectedItem2 === item ? null : item; // 切换选中状态
			if (this.selectedItem2 !== null) {
				this.cPersonCode = item.cPersonCode;
			} else {
				this.cPersonCode = '';
			}
			this.drawerRight2 = false; // 假设这是其他操作
		},
		drawerOpen2() {
			this.drawerRight2 = true;
		},
		drawerClose2() {
			this.drawerRight2 = false;
		},
		restSelectItem2() {
			this.selectedItem2 = null;
			this.cPersonCode = ''; // 清空
		},

		loadingMore() {
			if (this.noMoreData == true || this.loding == true) {
				return;
			}
			else if (this.tableData.length && this.loding == false) {
				this.getTableData();
			}
		},
		textFocus(e) {
			// 获取键盘高度
			const keyboardHeight = e.detail.height;

			// 动态设置输入框的底部距离，避免被键盘遮挡
			this.inputBoxStyle = `bottom:${keyboardHeight}px`;

			// 计算 scroll-view 高度，防止内容被遮挡
			this.scrollViewHeight = `calc(100vh - ${keyboardHeight}px)`;
		},
		textBlur() {
			// 失去焦点时恢复原位置
			this.inputBoxStyle = 'bottom:0px';

			// 恢复 scroll-view 高度
			this.scrollViewHeight = '100vh';
		},

		startConfirm() {
			setTimeout(() => {
				// 格式化日期为 'YYYY-MM-DD' 格式
				const date = new Date(this.startTimeCode);  // 将时间码转换为 Date 对象
				date.setDate(date.getDate() + 1);  // 在当前日期基础上加 1 天
				const formattedDate = date.toISOString().split('T')[0];  // 提取日期部分
				this.startTime = formattedDate;  // 将格式化后的日期赋值给 startTime
				console.log(formattedDate);  // 输出日期格式，调试时使用
			}, 500);
			this.startShow = false;  // 关闭日期选择器
		},
		startCancel() {
			this.startShow = false;
		},
		endCancel() {
			this.endShow = false;
		},
		endConfirm() {
			setTimeout(() => {
				// 格式化日期为 'YYYY-MM-DD' 格式
				const date = new Date(this.endTimeCode);  // 将时间码转换为 Date 对象
				date.setDate(date.getDate() + 1);  // 在当前日期基础上加 1 天
				const formattedDate = date.toISOString().split('T')[0];  // 提取日期部分
				this.endTime = formattedDate;  // 将格式化后的日期赋值给 endTime
				console.log(formattedDate);  // 输出日期格式，调试时使用
			}, 500);
			this.endShow = false;  // 关闭日期选择器
		},
		endCancel() {
			this.endShow = false;
		},
		comboxPerson(inputValue) {
			this.getPersonList(inputValue);
		},

		// 点击单元格时显示浮动窗
		showPopup(title, content) {
			this.popupTitle = title;
			this.popupContent = content;
			this.popupVisible = true;
		},
		// 关闭浮动窗
		closePopup() {
			this.popupVisible = false;
		},

		// 生成动态日期列
		generateDynamicDates(startDate, endDate) {
			const dates = [];
			let currentDate = new Date(startDate);
			const end = new Date(endDate);

			while (currentDate <= end) {
				const month = currentDate.getMonth() + 1; // 获取月份，注意 JavaScript 中月份是从 0 开始的
				const day = currentDate.getDate(); // 获取日期
				const formattedDate = `${month.toString().padStart(2, '0')}-${day.toString().padStart(2, '0')}`; // 格式化为 MM-DD
				dates.push(formattedDate);
				currentDate.setDate(currentDate.getDate() + 1); // 增加一天
			}

			this.dynamicDates = dates;
		},

		// 查询方法
		async search() {

			if (!this.startTime || !this.endTime) {
				uni.showToast({
					title: '请选择开始日期和结束日期',
					icon: 'none'
				});
				return;
			}
			this.searchClose();
			this.noMoreData = false;
			this.currentPage = 1;
			// 生成动态日期列
			this.generateDynamicDates(this.startTime, this.endTime);
			// 调用接口获取数据
			this.tableData = [];
			await this.getTableData();
		},
		// 获取表格数据
		async getTableData() {
			if (!this.loding) {
				this.loading = true;
				this.loding = true;
				try {
					const res = await GetOrderByHY({
						UserId: this.userInfo.UserId,
						OnlineCode: this.userInfo.OnlineCode,
						UserName: this.userInfo.UserName,  //登录用户名称
						Openid: this.userInfo.Openid,
						mobile: this.userInfo.mobile,   //手机号
						StartTime: this.startTime, // 开始日期
						EndTime: this.endTime, // 结束日期
						cVenCode: this.cVenCode, // 供应商
						cInvCode: this.cInvCode, // 物料
						searchvalue: this.searchvalue, // 模糊查询值
						cInvDefine2: this.cInvDefine2, // 材料
						cInvscx: this.cInvscx, // 生产线
						cPersonCode: this.cPersonCode, // 配套员
						isSHState: this.isSHState, // 是否只看送货
						isCQState: this.isCQState, // 是否只看查缺
						isLLState: this.isLLState,
						isXCState: this.isXCState,
						page: this.currentPage,
						pagesize: this.pageSize
					});
					if (res.success) {
						if (res.subdata.length > 0) {
							this.tableData = this.tableData.concat(res.subdata);
							this.currentPage++;
						} else {
							uni.showToast({
								title: '暂无更多数据',
								icon: 'none'
							});
							this.noMoreData = true;
						}
					} else {
						console.log("请求异常:", res.message);
						uni.showToast({
							title: '数据加载失败',
							icon: 'none'
						});
					}
				} catch (error) {
					uni.showToast({ title: '网络请求失败', icon: 'none' });
				} finally {
					this.toUp();
					this.loading = false;
					this.loding = false;
				}
			}
		},
		//产品下拉树
		async getInvList() {
			const res = await invList({
				UserId: this.userInfo.UserId,
				OnlineCode: this.userInfo.OnlineCode,
				cBusType: '工序委外',
				page: "1",
				pagesize: "100",
				cInvCode: this.searchInput
			});

			if (res.success) {
				if (res.subdata.length !== 0) {
					this.invList = [];
					this.invList = res.subdata;
				} else {
					uni.showToast({ title: '暂无相关产品', icon: 'none' })
				}
			}
		},
		//供应商下拉树
		async getVenList(cVenCode) {
			const res = await venList({
				UserId: this.userInfo.UserId,
				OnlineCode: this.userInfo.OnlineCode,
				cBusType: '工序委外',
				page: "1",
				pagesize: "100",
				cVenCode: cVenCode
			});

			if (res.success) {
				if (res.subdata.length !== 0) {
					this.venList = [];
					this.venList = res.subdata;
				} else {
					uni.showToast({ title: '暂无相关供应商', icon: 'none' })
				}
			}
		},

		//配套员下拉树
		async getPersonList(cVenCode) {
			const res = await personList({
				UserId: this.userInfo.UserId,
				OnlineCode: this.userInfo.OnlineCode,
			});

			if (res.success) {
				this.personList = [];
				this.personList = res.subdata;
			} else {
				this.personList = [];
			}
		},
		//重置筛选条件
		rest() {
			this.getDefaultDateRange();
			this.selectedItem2 = null;
			this.selectedItem1 = null;
			this.selectedItem = null;
			this.cVenCode = "";
			this.cInvCode = "";
			this.searchvalue = ""; //模糊查询值
			this.cInvDefine2 = "";//材料
			this.cInvscx = ""; 	 //产线
			this.cPersonCode = ""; //配套员
			this.isSHState = "0";  //是否只看送货
			this.isCQState = "0"; //是否只看查缺
			this.isLLState = "0";
			this.isXCState = "0";
		},
		//关闭弹窗
		searchClose() {
			this.searchOpen = false
		},
		//时间条件
		dateChange(newDatetime) {
			if (newDatetime && newDatetime.length == 2) {
				this.startTime = newDatetime[0];
				this.endTime = newDatetime[1];
			} else {
				this.startTime = '';
				this.endTime = '';
			}
		},
		//设置默认时间
		getDefaultDateRange() {
			const today = new Date();
			const startOfDay = new Date(today.setHours(0, 0, 0, 0)); // 今天0点
			const endOfDay = new Date(startOfDay);
			endOfDay.setDate(endOfDay.getDate() + 7); // 后7天的0点	
			// 格式化为 ISO 字符串（例如：2025-02-07T00:00:00.000Z）
			const formatDate = (date) => {
				return `${date.getFullYear()}-${(date.getMonth() + 1).toString().padStart(2, "0")}-${date.getDate().toString().padStart(2, "0")}`;
			};

			this.startTime = formatDate(startOfDay);
			this.endTime = formatDate(endOfDay);
		},
		//绑定初始时间
		initDateTime() {
			if (this.dateTime && this.dateTime.length === 2) {
				this.startTime = this.dateTime[0];
				this.endTime = this.dateTime[1];
			} else {
				this.startTime = "";
				this.endTime = "";
			}
		},
		toggleSHState() {
			this.isSHState = this.isSHState === '0' ? '1' : '0';
		},
		toggleCQState() {
			this.isCQState = this.isCQState === '0' ? '1' : '0';
		},
		toggleLLState() {
			this.isLLState = this.isLLState === '0' ? '1' : '0';
		},
		toggleXCState() {
			this.isXCState = this.isXCState === '0' ? '1' : '0';
		},
		toUp() {
			// 获取页面滚动位置
			const query = uni.createSelectorQuery().in(this);
			query.select('.uni-container').boundingClientRect((rect) => {
				if (rect) {
					// 获取当前页面的滚动位置
					uni.createSelectorQuery().selectViewport().scrollOffset(res => {
						const currentScrollTop = res.scrollTop;

						// 如果当前已经在顶部，则不执行滚动
						if (currentScrollTop <= 0) {
							console.log("已经在顶部，不再滚动");
							return;
						}

						// 调整滚动位置，向上滚动50px
						uni.pageScrollTo({
							scrollTop: rect.top - 2, // 向上滚动50px
							duration: 300, // 滚动的动画时长
						});
					}).exec();
				}
			}).exec();
		}
	},
	// 触底函数
	onReachBottom() {

	}
}
</script>

<style lang="less" scoped>
.msgView {
	width: 100%;
	height: 5vh;
	display: flex;
	/* 防止换行 */
	flex-wrap: nowrap;
}

.msg {
	display: flex;
	justify-content: center;
	/* 水平方向居中 */
	align-items: center;
	/* 垂直方向居中 */
	margin-left: 10rpx;
	font-size: 11rpx;
}

::v-deep .uni-drawer {

	.uni-calendar__mask,
	.uni-calendar--fixed {
		width: 100vw;
	}
}


.popup {
	position: fixed;
	top: 0;
	left: 0;
	width: 100%;
	height: 100%;
	background: rgba(0, 0, 0, 0);
	display: flex;
	justify-content: center;
	align-items: center;
	font-size: x-small;
}

.popup-content {
	background: white;
	padding: 20px;
	border-radius: 8px;
	width: 80%;
	max-width: 500px;
}

.popup-header {
	display: flex;
	justify-content: space-between;
	align-items: center;
}

.popup-body {
	margin-top: 10px;
}

button {
	padding: 5px 10px;
	cursor: pointer;
}


// .card {
// 	margin-bottom: 30rpx;
// 	padding: 0rpx 30rpx;
// 	border-radius: 0.5rem;
// 	// box-shadow: 0px 187px 75px rgba(0, 0, 0, 0.01), 0px 105px 63px rgba(0, 0, 0, 0.05), 0px 47px 47px rgba(0, 0, 0, 0.09), 0px 12px 26px rgba(0, 0, 0, 0.1), 0px 0px 0px rgba(0, 0, 0, 0.1);
// 	background-color: #FFFFFF;
// }

.searchTitle {
	display: flex;
	justify-content: center;
	align-items: center;
	color: black;
	font-weight: bold;
	font-size: 10px;
	margin-bottom: 2rpx;
}

.uni-col-view {
	font-size: 12rpx;
	height: 30rpx;
	border-radius: 5px;
	display: flex;
	// justify-content: center;
	align-items: center;
}

.drawer {
	padding: 10rpx;
	width: 100%;
}

.warp {
	display: flex;
	align-items: center;
	justify-content: center;
	height: 100%;
}

.rect {
	width: 120px;
	height: 120px;
	display: flex;
	/* 启用flex布局 */
	justify-content: center;
	/* 水平居中对齐 */
	align-items: center;
	// background-color: #fff;
}

/* From Uiverse.io by abrahamcalsin */
.dot-spinner {
	--uib-size: 2.8rem;
	--uib-speed: .9s;
	--uib-color: #43C6AC;
	position: relative;
	display: flex;
	align-items: center;
	justify-content: flex-start;
	height: var(--uib-size);
	width: var(--uib-size);
}

.dot-spinner__dot {
	position: absolute;
	top: 0;
	left: 0;
	display: flex;
	align-items: center;
	justify-content: flex-start;
	height: 100%;
	width: 100%;
}

.dot-spinner__dot::before {
	content: '';
	height: 20%;
	width: 20%;
	border-radius: 50%;
	background-color: var(--uib-color);
	transform: scale(0);
	opacity: 0.5;
	animation: pulse0112 calc(var(--uib-speed) * 1.111) ease-in-out infinite;
	box-shadow: 0 0 20px rgba(18, 31, 53, 0.3);
}

.dot-spinner__dot:nth-child(2) {
	transform: rotate(45deg);
}

.dot-spinner__dot:nth-child(2)::before {
	animation-delay: calc(var(--uib-speed) * -0.875);
}

.dot-spinner__dot:nth-child(3) {
	transform: rotate(90deg);
}

.dot-spinner__dot:nth-child(3)::before {
	animation-delay: calc(var(--uib-speed) * -0.75);
}

.dot-spinner__dot:nth-child(4) {
	transform: rotate(135deg);
}

.dot-spinner__dot:nth-child(4)::before {
	animation-delay: calc(var(--uib-speed) * -0.625);
}

.dot-spinner__dot:nth-child(5) {
	transform: rotate(180deg);
}

.dot-spinner__dot:nth-child(5)::before {
	animation-delay: calc(var(--uib-speed) * -0.5);
}

.dot-spinner__dot:nth-child(6) {
	transform: rotate(225deg);
}

.dot-spinner__dot:nth-child(6)::before {
	animation-delay: calc(var(--uib-speed) * -0.375);
}

.dot-spinner__dot:nth-child(7) {
	transform: rotate(270deg);
}

.dot-spinner__dot:nth-child(7)::before {
	animation-delay: calc(var(--uib-speed) * -0.25);
}

.dot-spinner__dot:nth-child(8) {
	transform: rotate(315deg);
}

.dot-spinner__dot:nth-child(8)::before {
	animation-delay: calc(var(--uib-speed) * -0.125);
}

@keyframes pulse0112 {

	0%,
	100% {
		transform: scale(0);
		opacity: 0.5;
	}

	50% {
		transform: scale(1);
		opacity: 1;
	}
}

::v-deep .load-more {
	text-align: center;
	color: #999;
	font-size: 18rpx;
}

::v-deep .fixed-column {
	background-color: #45b649;
	padding: 0;
	position: sticky;
	left: 0;
	background-color: white;
	// z-index: 5;
}

.table-thead {
	position: sticky;
	left: 100rpx;
	top: 0;
	z-index: 20;
}

/* 设置表格的宽度，确保内容超出容器宽度 */
::v-deep .uni-table {
	// width: 1000px;
	// /* 或根据需要设置具体宽度 */
	// table-layout: fixed;
	// /* 确保表格布局固定 */
	position: relative;
	min-width: 100%;
	table-layout: fixed;
	border-collapse: separate;
	border-spacing: 0;
	/* 消除单元格间隙 */
}

::v-deep .uni-table-tr {
	overflow: visible;
	background-color: #fff;
}

// //固定表头第一列
// ::v-deep .uni-table-tr .uni-table-th:first-child {
// 	position: sticky;
// 	left: 0;
// 	top: 0;
// 	background-color: #FFFAF2;
// 	z-index: 10;
// }

// //冻结thead第一列
// ::v-deep .uni-table-tr .uni-table-td:first-child {
// 	position: sticky;
// 	left: 0;
// 	top: 0;
// 	background-color: #fff;
// 	z-index: 10;
// }

/* 表头双重冻结（上下+左右） */
::v-deep .tableHead {
	.uni-table-tr {
		position: sticky;
		top: 0;
		/* 关键：固定表头在顶部 */
		z-index: 100;
		/* 最高层级 */
		background: #f4f6ff;
	}

	// .uni-table-th:nth-child(-n+4) {
	// 	position: sticky;
	// 	z-index: 100;
	// 	background: #f4f6ff !important;

	// 	/* 列定位 */
	// 	&:nth-child(1) {
	// 		left: 0;
	// 	}

	// 	&:nth-child(2) {
	// 		left: 27rpx;
	// 	}

	// 	&:nth-child(3) {
	// 		left: 123rpx;
	// 	}

	// 	&:nth-child(4) {
	// 		left: 220rpx;
	// 	}
	// }
}

/* 内容区前四列冻结 */
// ::v-deep .tableBody {

// 	.uni-table-td:nth-child(-n+4) {
// 		position: sticky;
// 		z-index: 50;
// 		/* 低于表头 */
// 		background: white;

// 		/* 列定位 */
// 		&:nth-child(1) {
// 			left: 0;
// 		}

// 		&:nth-child(2) {
// 			left: 27rpx;
// 		}

// 		&:nth-child(3) {
// 			left: 123rpx;
// 		}

// 		&:nth-child(4) {
// 			left: 220rpx;
// 		}
// 	}
// }

::v-deep .uni-table-td {
	// overflow: hidden;
	// text-overflow: ellipsis; // 添加省略号
	// white-space: nowrap;
	padding: 0rpx;
}

::v-deep .uni-table-th {
	padding: 3rpx;
}

/* 固定表头父级样式 */
// .uni-container {
// 	height: 100vh;
// 	margin-top: 5rpx;
// 	position: relative;
// }

/* //表头固定样式 */
::v-deep .tableHead {
	font-weight: bold;
	color: #333333;
	background: #F4F6FF;
	z-index: 5;
	position: absolute;
	top: 0rpx;
}

::v-deep .tableBody {
	height: 87vh;
	overflow: scroll;
	margin-top: 24px;
	// background-color: white;
	background: #F4F6FF;
	z-index: 5;
	// position: absolute;
}

/* 表格容器 */
.uni-container {
	height: 100vh;
	overflow: auto;
	-webkit-overflow-scrolling: touch;
	/* iOS弹性滚动 */
}

// /* 表格全局样式 */
// ::v-deep .uni-table {
// 	position: relative;
// 	min-width: 100%;
// 	table-layout: fixed;
// 	border-collapse: separate;
// 	border-spacing: 0;
// 	/* 消除单元格间隙 */
// }

// /* 表头双重冻结（上下+左右） */
// ::v-deep .tableHead {
// 	.uni-table-tr {
// 		position: sticky;
// 		top: 0;
// 		/* 关键：固定表头在顶部 */
// 		z-index: 100;
// 		/* 最高层级 */
// 		background: #f4f6ff;
// 	}

// 	.uni-table-th:nth-child(-n+4) {
// 		position: sticky;
// 		z-index: 100;
// 		background: #f4f6ff !important;

// 		/* 列定位 */
// 		&:nth-child(1) {
// 			left: 0;
// 		}

// 		&:nth-child(2) {
// 			left: 27rpx;
// 		}

// 		&:nth-child(3) {
// 			left: 123rpx;
// 		}

// 		&:nth-child(4) {
// 			left: 220rpx;
// 		}
// 	}
// }

// /* 内容区前四列冻结 */
// ::v-deep .tableBody {
// 	.uni-table-td:nth-child(-n+4) {
// 		position: sticky;
// 		z-index: 50;
// 		/* 低于表头 */
// 		background: white;

// 		/* 列定位 */
// 		&:nth-child(1) {
// 			left: 0;
// 		}

// 		&:nth-child(2) {
// 			left: 27rpx;
// 		}

// 		&:nth-child(3) {
// 			left: 123rpx;
// 		}

// 		&:nth-child(4) {
// 			left: 220rpx;
// 		}
// 	}
// }

// /* 边框增强 */
// ::v-deep .uni-table {

// 	.uni-table-th:nth-child(-n+4),
// 	.uni-table-td:nth-child(-n+4) {
// 		&::after {
// 			content: '';
// 			position: absolute;
// 			right: 0;
// 			top: 0;
// 			height: 100%;
// 			width: 1px;
// 			background: #ebeef5;
// 		}
// 	}
// }

// /* 文本溢出处理 */
// ::v-deep .uni-table-td {
// 	padding: 0;
// 	// overflow: hidden;
// 	// text-overflow: ellipsis;
// 	// white-space: nowrap;
// }

// ::v-deep .uni-table-th {
// 	padding: 0;
// }


/* 固定在右下角的样式 */
.fixed-bottom-right {
	position: fixed;
	bottom: 20px;
	/* 距离底部 20px */
	left: 30px;
	/* 距离右侧 20px */
	z-index: 100;
	/* 确保按钮在最上层 */
}

.fixed-bottom-left {
	position: fixed;
	bottom: 20px;
	/* 距离底部 20px */
	right: 80px;
	/* 距离右侧 20px */
	z-index: 100;
	/* 确保按钮在最上层 */
}

.td-full {
	width: 100%;
	height: 100%;
	display: flex;
	align-items: center;
	/* 垂直居中 */
	justify-content: flex-end;
	/* 水平靠右 */
}

.txt {
	color: black;
	font-weight: normal;
	font-style: italic;
	text-decoration: underline;
}

.bg-green {
	background-color: #45b649 !important;
	color: black;
	/* 绿色 */
}

.bg-yellow {
	background-color: #fffc00 !important;
	color: black;
	/* 黄色 */
}

.bg-red {
	background-color: #f64f59 !important;
	color: black;
	/* 红色 */
}

.uni-row {
	margin-top: 0rpx;
}

.row-height {
	margin-top: 40rpx;
}

::v-deep .uni-col {
	height: 35rpx;
	border-radius: 5px;
	display: flex;
	justify-content: flex-end;
	/* 元素靠右对齐 */
	align-items: center;
}

//其他样式
.grid {
	display: grid;
	grid-template-columns: repeat(4, 1fr);
	gap: 10px;
}

.padding {
	// padding: 5rpx 10rpx;
}

.bg-white {
	background-color: white;
}

//选择器样式
._container {
	margin-top: 10rpx;
	height: 100vh;
	display: flex;
	flex-direction: column;
	background: #f8f9fa;
	padding: 15rpx;
}

.search-header {
	width: 90%;
	padding: 10rpx 0;
	background: #fff;
	position: sticky;
	top: 20;
	z-index: 10;
}

.search-box {
	display: flex;
	align-items: center;
	gap: 10rpx;
	padding: 0 10rpx;
}

.search-input {
	flex: 1;
	font-size: 28rpx;
}

.action-btn {
	width: 120rpx;
	font-size: 24rpx;
}

.result-list {
	flex-grow: 1;
	overflow-y: auto;
	margin-top: 15rpx;
}

.result-row {
	display: flex;
	flex-wrap: wrap;
	/* 允许换行 */
	gap: 10rpx;
	/* 设定项目之间的间距 */
}

.result-item {
	flex: 0 0 32%;
	/* 每行显示三个，且每个项占 32% 的宽度 */
	padding: 10rpx;
	background: #fff;
	margin: 8rpx 0;
	border-radius: 8rpx;
	box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
	font-size: 10rpx;
	cursor: pointer;
}

.result-item.selected {
	background: #007BFF !important;
	/* 确保覆盖默认背景色 */
	color: #fff;
}

.no-data {
	text-align: center;
	margin-top: 15rpx;
	color: #aaa;
	font-size: 26rpx;
}
</style>
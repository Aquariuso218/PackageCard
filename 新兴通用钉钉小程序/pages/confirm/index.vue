<template>
	<view class="_content" style="padding: 5rpx 20rpx 0rpx 20rpx;">
		<view class="topBox">
			<uni-row class="uni-row">
				<uni-col style="background-color: chocolate;" class="custom-col" :span="6">
					<view class="uni-col_search">入库仓库:</view>
				</uni-col>
				<uni-col style="height: 80rpx;" :span="18" class="divRight custom-col">
					<view class="uni-col-view" @click="drawerOpen1"
						style="width: 100%; border: 1rpx solid #bdc3c7;padding-left: 9rpx;">{{ selectedItem1.cWhName || '请点击选择'
							}} </view>
				</uni-col>
			</uni-row>
		</view>
		<view class="_content" style="padding-top: 20rpx;">
			<view v-for="(item, index) in listData" :key="item.id"
				:class="['card', { 'card-gray': item.isFlag === 1, 'selected': isSelected(item.id) }]"
				@click="toggleCancel(item.id)">
				<uni-row class="uni-row">
					<uni-col :span="8">
					  <view class="uni-col textStyle" :style="{ 'background-color': item.isChange === 1 ? '#FF4B2B' : '' }">{{ item.invCode }}</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view class="uni-col">{{ item.invName }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>箱号:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.boxNumber }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>流转卡:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.flowCard }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>产品id:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.invID }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>产品图号:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.invAddCode }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>数量:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.quantity }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>时间:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.reportTime }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="8">
						<view>状态:</view>
					</uni-col>
					<uni-col :span="16" class="textRight">
						<view>{{ item.isFlag === 1 ? '已入库' : '未入库' }}</view>
					</uni-col>
				</uni-row>
			</view>
			<view v-if="!listData.length" style="height: 85vh;">
				<u-empty :mode="'data'"></u-empty>
			</view>
		</view>
		<view style="height: 95rpx;"></view>
		<view class="sureBtn">
			<view style="width: 80%;">
				<u-button size="large" text="产品入库" color="#43C6AC" @click="stockIn"></u-button>
			</view>
		</view>
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
		<u-popup :customStyle="select_popupStyle" :show="drawerRight1" :closeable="true" @close="drawerClose1"
			mode="bottom" :round="10">
			<view class="_container">
				<view class="search-header">
					<view class="search-box">
						<view style="width: 80%;">
							<u--input placeholder="请输入仓库编码或名称" border="surround" clearable v-model="searchInput1"
								suffixIcon="search" iconStyle="color: #909399;" class="search-input"
								@input="getWareHoseList"></u--input>
						</view>
						<view style="width: 20%;">
							<u-button text="重置" @click="restSelectItem" class="action-btn"></u-button>
						</view>
					</view>
				</view>
				<view class="result-list" v-if="wareHouseList.length">
					<view v-for="(item, index) in wareHouseList" :key="index"
						:class="['result-item', { 'selected': selectedItem1.cWhCode === item.cWhCode }]"
						@click="selectItem1(item)">
						<text>{{ item.cWhCode }} - {{ item.cWhName }}</text>
					</view>
				</view>
				<view v-else class="no-data">
					<text>没有找到数据</text>
				</view>
			</view>
		</u-popup>
		<u-popup :customStyle="{height: '30vh'}" :show="drawer_SX" :closeable="false" @close="drawer_SX_Close"
			mode="top">
			<view style="padding: 5px;">
				<u-row customStyle="margin-bottom: 5px">
					<u-col span="3">
						<view class="demo-layout textView">日期:</view>
					</u-col>
					<u-col span="9">
						<view class="demo-layout">
							<uni-datetime-picker v-model="datetimerange" type="daterange" rangeSeparator="至"
								@change="onDateTimeChange" />
						</view>
					</u-col>
				</u-row>
				<u-row customStyle="margin-bottom: 5px">
					<u-col span="3">
						<view class="demo-layout textView">物料编码:</view>
					</u-col>
					<u-col span="9">
						<view class="demo-layout">
							<u--input v-model="params.invCode" placeholder="请输入物料编码" />
						</view>
					</u-col>
				</u-row>
				<u-row customStyle="margin-bottom: 5px">
					<u-col span="3">
						<view class="demo-layout textView">关键字:</view>
					</u-col>
					<u-col span="9">
						<view class="demo-layout">
							<u--input v-model="params.keyWord" placeholder="请输入关键字" />
						</view>
					</u-col>
				</u-row>
				<u-row customStyle="margin-bottom: 5px;margin-top: 5px;">
					<u-col span="6">
						<view class="demo-layout SXbtn">
							<u-button type="primary" color="#D7DDE8" size="small" text="重置"
								@click="resetFilters"></u-button>
						</view>
					</u-col>
					<u-col span="6">
						<view class="demo-layout SXbtn">
							<u-button type="primary" size="small" text="查询" @click="searchWithFilters"></u-button>
						</view>
					</u-col>
				</u-row>
			</view>
		</u-popup>
		<view class="float-btn">
			<u-button type="primary" text="扫码" @click="scanCode" shape="circle"
				custom-style="width: 130rpx; height: 130rpx; font-size: 30rpx;"></u-button>
		</view>
		<view class="float-btn" style="bottom: 30%">
			<u-button type="primary" text="筛选" @click="drawer_SX_open" shape="circle"
				custom-style="width: 130rpx; height: 130rpx; font-size: 30rpx;"></u-button>
		</view>
	</view>
</template>

<script>
	import {
		stockInList,
		stockIn,
		wareHouseList,
		getStockInList
	} from "@/nxTemp/apis/sunset.js"
	export default {
		data() {
			return {
				select_popupStyle: {
					width: '100vw',
					backgroundColor: '#ffffff',
				},
				wareHouseList: [],
				searchInput1: '',
				selectedItem1: null,
				cWhCode: "",
				drawerRight1: false,
				userInfo: [],
				barCode: '',
				loding: false,
				drawer_SX: false,
				datetimerange: [
					new Date().toISOString().split('T')[0],
					new Date().toISOString().split('T')[0]
				],
				selectedIDs: [],
				listData: [],
				stockInData: [],
				params: {
					startTime: new Date().toISOString().split('T')[0],
					endTime: new Date().toISOString().split('T')[0],
					invCode: "",
					keyWord: ""
				}
			};
		},
		onLoad() {
			setTimeout(() => {
				this.userInfo = uni.getStorageSync('userInfo');
				this.toStockInList(this.params);
				this.getWareHoseList();
			}, 1000);
		},
		methods: {
			scanCode() {
				const that = this;
				uni.scanCode({
					onlyFromCamera: true,
					success: function(res) {
						that.barCode = res.result;
						if (that.barCode) {
							that.getList(that.barCode);
						} else {
							uni.showToast({
								title: '无效的条码!',
								icon: 'none'
							});
						}
					}
				});
			},
			async toStockInList(params) {
				this.loding = true;
				try {
					const res = await getStockInList(params);
					if (res.code == 1) {
						this.listData = res.data;
						console.log(this.listData);
						// 重新匹配选中状态
						this.matchAndSelect();
					} else {
						this.listData = [];
						uni.showToast({
							title: res.msg,
							icon: 'none'
						});
					}
				} catch (err) {
					uni.showToast({
						title: '网络请求失败！',
						icon: 'none'
					});
				}
				this.loding = false;
			},
			async getList(code) {
				this.loding = true;
				try {
					const res = await stockInList(code);
					if (res.code === 1) {
						this.stockInData = res.data || [];
						this.matchAndSelect();
					} else {
						uni.showToast({
							title: res.msg || '查询失败',
							icon: 'none'
						});
					}
				} catch (err) {
					uni.showToast({
						title: '网络请求失败！',
						icon: 'none'
					});
				}
				this.loding = false;
			},
			//可优化：根据流转卡匹配数据！
			matchAndSelect() {
				// 清空之前的选中状态
				this.selectedIDs = [];
				// 匹配 stockInData 和 listData，仅选择未入库的数据
				this.stockInData.forEach(stockItem => {
					const matchedItem = this.listData.find(listItem => listItem.id === Number(stockItem.id));
					if (matchedItem && matchedItem.isFlag !== 1 && !this.selectedIDs.includes(matchedItem.id)) {
						this.selectedIDs.push(matchedItem.id);
					}
				});
				this.selectedIDs = [...this.selectedIDs];

				// 重新排序 listData，将 selectedIDs 中的数据置顶
				this.listData = [
					...this.listData.filter(item => this.selectedIDs.includes(item.id)), // 匹配到的数据放前面
					...this.listData.filter(item => !this.selectedIDs.includes(item.id)) // 未匹配的数据放后面
				];
			},
			async stockIn() {
				this.loding = true;
				if (!this.listData.length) {
					uni.showToast({
						title: '请扫码查询入库数据！',
						icon: 'none'
					});
					this.loding = false;
					return;
				}
				if (!this.userInfo || !this.userInfo.name) {
					uni.showToast({
						title: '未获取到您的用户信息！',
						icon: 'none'
					});
					this.loding = false;
					return;
				}
				const stockData = this.listData.filter(item =>
					this.selectedIDs.includes(item.id) && item.isFlag === 0
				);
				if (!stockData.length) {
					uni.showToast({
						title: '没有需要入库的产品数据！',
						icon: 'none'
					});
					this.loding = false;
					return;
				}
				try {
					const res = await stockIn({
						cMaker: this.userInfo.name,
						cWhCode: this.cWhCode,
						PCDList: stockData
					});
					if (res.code === 1) {
						uni.showToast({
							title: '产品入库成功！',
							icon: 'success'
						});
						this.listData = [];
						this.toStockInList(this.params); // 重新查询列表
						this.selectedIDs = [];
					} else {
						uni.showToast({
							title: res.msg || '入库失败！',
							icon: 'none'
						});
					}
				} catch (err) {
					uni.showToast({
						title: '网络请求失败！',
						icon: 'none'
					});
				}
				this.loding = false;
			},
			onDateTimeChange(e) {
				if (e && e.length === 2) {
					this.datetimerange = e;
					this.params.startTime = e[0];
					this.params.endTime = e[1];
				}
			},
			searchWithFilters() {
				this.toStockInList(this.params);
				this.drawer_SX = false;
			},
			resetFilters() {
				const today = new Date().toISOString().split('T')[0];
				this.datetimerange = [today, today];
				this.params = {
					startTime: today,
					endTime: today,
					invCode: "",
					keyWord: ""
				};
			},
			drawer_SX_open() {
				this.drawer_SX = true;
			},
			drawer_SX_Close() {
				this.drawer_SX = false;
			},
			async getWareHoseList(keyWords) {
				try {
					const res = await wareHouseList({
						PageNum: 1,
						PageSize: 100,
					}, keyWords);
					if (res.code == 200) {
						this.wareHouseList = res.data.result;
					} else {
						this.wareHouseList = [];
					}
				} catch (err) {
					this.wareHouseList = [];
				}
			},
			async selectItem1(item) {
				this.selectedItem1 = this.selectedItem1 === item ? null : item;
				this.cWhCode = this.selectedItem1 ? item.cWhCode : '';
				this.drawerRight1 = false;
			},
			restSelectItem() {
				this.selectedItem1 = null;
				this.cWhCode = '';
			},
			drawerOpen1() {
				this.drawerRight1 = true;
			},
			drawerClose1() {
				this.drawerRight1 = false;
			},
			toggleCancel(id) {
				// 仅允许取消已选中的未入库数据
				if (this.selectedIDs.includes(id)) {
					const item = this.listData.find(item => item.id === id);
					if (item && item.isFlag !== 1) {
						this.selectedIDs = this.selectedIDs.filter(selectedId => selectedId !== id);
					}
				}
			},
			isSelected(id) {
				return this.selectedIDs.includes(id);
			},
		},
	};
</script>


<style lang="less" scoped>
	/deep/ .bg-red {
	  background-color: #FF4B2B !important; /* 行背景色 */
	}

	.textView {
		display: flex;
		justify-content: flex-end;
		align-items: center;
		font-size: 35rpx;
	}

	.SXbtn {
		padding: 0rpx 30rpx;
	}

	.topBox {
		margin: 10rpx;
	}

	.wrap {
		padding: 12px;
	}

	.demo-layout {
		height: 40px;
		border-radius: 4px;
	}

	.bg-purple {
		background: #CED7E1;
	}

	.bg-purple-light {
		background: #e5e9f2;
	}

	.bg-purple-dark {
		background: #99a9bf;
	}


	.uni-col_search {
		height: 60rpx;
		/* 设置高度 */
		display: flex;
		/* 使 view 成为 Flex 容器 */
		align-items: center;
		/* 垂直居中 */
		justify-content: flex-start;
		/* 水平靠左，可根据需要调整为 center */
	}

	.custom-col {
		display: flex;
		/* 确保 uni-col 使用 Flex 布局 */
		align-items: center;
		/* 垂直居中 */
		justify-content: flex-start;
		/* 水平方向靠左，也可以根据需要调整 */
	}

	/* 添加悬浮按钮样式 */
	.float-btn {
		position: fixed;
		bottom: 20%;
		/* 距离底部距离，避开底部的 sureBtn */
		right: 5%;
		/* 距离右侧距离 */
		z-index: 999;
		/* 确保按钮在其他内容之上 */
	}

	/* 调整 u-button 的圆形样式（可选，通过 custom-style 已设置） */
	/deep/ .u-button {
		display: flex;
		justify-content: center;
		align-items: center;
	}

	.load-more {
		text-align: center;
		padding: 20rpx;
		color: #999;
		font-size: 28rpx;
	}

	.uni-col-view {
		font-size: 26rpx;
		height: 60rpx;
		border-radius: 5px;
		display: flex;
		// justify-content: center;
		align-items: center;
	}

	//选择器样式
	._container {
		margin-top: 10rpx;
		height: 65vh;
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
		font-size: 40rpx;
		cursor: pointer;
		text-align: center;
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

	/* From Uiverse.io by akshat-patel28 */
	.loader {
		position: relative;
		overflow: hidden;
		border-right: 3px solid;
		width: 0px;
		animation: typewriter 2s steps(10) infinite alternate, blink 0.5s steps(10) infinite;
	}

	.loader-text {
		font-size: 40px;
		font-weight: 700;
		background: linear-gradient(to right, #cb2d3e, #ef473a);
		-webkit-background-clip: text;
		-webkit-text-fill-color: transparent;
	}

	@keyframes typewriter {
		0% {
			width: 0px;
		}

		100% {
			width: 240px;
		}
	}

	@keyframes blink {
		0% {
			border-right-color: rgba(255, 255, 255, .75);
			;
		}

		100% {
			border-right-color: transparent;
		}
	}

	.empty {
		width: 100%;
		min-height: 1100rpx;
		display: flex;
		/* 启用flex布局 */
		justify-content: center;
		/* 水平居中对齐 */
		align-items: center;
	}

	._content {
		// min-height: auto;
		font-size: medium;
		background-color: #f5f4f4;
	}

	.card {
		margin-bottom: 30rpx;
		padding: 5rpx 30rpx;
		border-radius: 0.5rem;
		// box-shadow: 0px 187px 75px rgba(0, 0, 0, 0.01), 0px 105px 63px rgba(0, 0, 0, 0.05), 0px 47px 47px rgba(0, 0, 0, 0.09), 0px 12px 26px rgba(0, 0, 0, 0.1), 0px 0px 0px rgba(0, 0, 0, 0.1);
		background-color: #FFFFFF;
	}

	.card-gray {
		background-color: #D3D3D3;
		/* 灰色背景，用于 isFlag 为 1 的情况 */
	}

	.card.selected {
		background-color: #007BFF !important;
		color: #FFFFFF !important;

		.textStyle {
			color: #FFFFFF !important;
		}

		.uni-col {
			color: #FFFFFF !important;
		}
	}

	.uni-row {
		// 组件在小程序端display为inline
		// QQ、抖音小程序文档写有 :host，但实测不生效
		// 百度小程序没有 :host
		/* #ifdef MP-TOUTIAO || MP-QQ || MP-BAIDU */
		display: block;
		/* #endif */
	}



	.firstrow {
		height: 70rpx;
		padding-top: 10rpx;
		border-bottom: 2rpx solid rgba(0, 0, 0, 0.3);
	}

	.uni-col {
		height: 50rpx;
		border-radius: 5px;
		display: flex;
		// justify-content: center;
		align-items: center;
	}

	.example-body {
		/* #ifndef APP-NVUE */
		display: block;
		/* #endif */
		padding: 5rpx 10rpx 0;
		overflow: hidden;
	}

	.textStyle {
		font-weight: 1000;
		color: #43C6AC;
	}

	.textRight {
		display: flex;
		justify-content: flex-end;
	}

	.divRight {
		text-align: right;
		display: flex;
		justify-content: flex-end;
	}

	.number {
		margin-left: 3rpx;
		font-size: medium;
		font-weight: 1000;
	}

	.select {
		padding-top: 10rpx;
		padding-left: 30rpx;
		display: flex;
		bottom: 0;
	}

	.sureBtn {
		width: 100%;
		height: 110rpx;
		padding: 20rpx 30rpx 20rpx 30rpx;
		/* 假设这是小程序中的单位 */
		position: fixed;
		/* 使按钮固定在视口 */
		display: flex;
		justify-content: center;
		align-items: center;
		left: 50%;
		/* 将按钮的左边缘定位到屏幕宽度的 50% */
		transform: translateX(-50%);
		/* 水平居中 */
		bottom: 0;
		/* 距离屏幕底部 20px */
		z-index: 200;
		/* 确保它位于其他内容之上 */
		background-color: #fdf7f7;
		// background-color: rgba(0, 0, 0, 0.0); /* 半透明背景 */
		/* 按钮背景颜色 */
		font-size: 20px;
		font-weight: bold;
		font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
		color: #FFFFFF;
		/* 按钮文字颜色 */
		border-radius: 0.5rem;
		/* 圆角边框 */
	}

	.button-hover {
		background: #11998e;
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
</style>
<template>

	<view class="content">
		<!-- 选项卡1的内容-->
		<view v-if="!this.listData || this.listData.length === 0">
			<u-empty :mode="'data'" :icon="'https://cdn.uviewui.com/uview/demo/empty/data.png'">
			</u-empty>
		</view>
		<view v-if="this.listData.length !== 0">
			<view v-for="(item, index) in listData" :key="item.ID" class="card">
				<uni-row class="uni-row">
					<uni-col :span="12">
						<view>存货编码:</view>
					</uni-col>
					<uni-col :span="12" class="textRight">
						<view>{{ item.cInvCode }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="12">
						<view>存货名称:</view>
					</uni-col>
					<uni-col :span="12" class="textRight">
						<view>{{ item.cInvName }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="12">
						<view>规格型号:</view>
					</uni-col>
					<uni-col :span="12" class="textRight">
						<view>{{ item.cInvStd }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="12">
						<view>订单数量:</view>
					</uni-col>
					<uni-col :span="12" class="textRight textStyle">
						<view>{{ item.iQuantity }}</view>
					</uni-col>
				</uni-row>
				<uni-row class="uni-row">
					<uni-col :span="12">
						<view>到货日期:</view>
					</uni-col>
					<uni-col :span="12" class="textRight">
						<view>{{ item.dArriveDate.trim() }}</view>
					</uni-col>
				</uni-row>
			</view>
		</view>
		<view class="load-more">
			<text v-if="loading ">加载中...</text>
			<text v-else="noMoreData">没有更多数据了</text>
			<text v-if="this.listData.length > 10">上拉加载更多</text>
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
	</view>
</template>

<script>
import { orderDetailList } from "@/nxTemp/apis/sunset.js"
export default {
	data() {
		return {
			items: ['采购订单', '委外订单', '工序加工单'],
			current: 0,
			colorIndex: 0,
			activeColor: '#43C6AC',
			styleType: 'button',

			currentPage: 1,
			pageSize: 10,
			loading: false,
			loding: false,
			noMoreData: false,

			//接收参数
			id: undefined,
			type: undefined,
			userInfo: [],

			listData: [
				// { "ID": "1002195645", "POID": 1002195645, "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承", "cMemo": null }, { "ID": "1002195646", "POID": "1002195646", "cBusType": "普通采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承1", "cMemo": null }, { "ID": "1002195647", "POID": "1002195647", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }, { "ID": "1002195648", "POID": "1002195648", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }, { "ID": "1002195649", "POID": "1002195649", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }, { "ID": "1002195650", "POID": "1002195651", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }, { "ID": "1002195652", "POID": "1002195652", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }, { "ID": "1002195653", "POID": "10021956453", "cBusType": "代管采购", "cPOID": "CG25010639", "dPODate": "2025-01-21", "cVenCode": "04043", "cVenName": "常州苏特轴承2", "cMemo": null }
			],
		};
	},
	onLoad(options) {
		this.id = options.id;
		const type = options.type;
		this.type = decodeURIComponent(type);  // 解码为中文
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
				uni.reLaunch({
					url: '/pages/login/login'
				});
			}
		});
		setTimeout(() => {
			this.loding = true;
			this.getdetail();
		}, 1000);
	},
	methods: {
		async getdetail() {
			this.loading = true;
			try {
				const res = await orderDetailList({
					UserId: this.userInfo.UserId,
					OnlineCode: this.userInfo.OnlineCode,
					cBusType: this.type,
					ID: this.id,
					page: this.currentPage,
					pagesize: this.pageSize,
				});

				if (res.success) {
					if (res.subdata.length > 0) {
						this.listData = this.listData.concat(res.subdata);
						this.currentPage++;
					} else {
						this.noMoreData = true;
					}
					//控制数据为空时的动画显示
					this.nodata = this.listData.length === 0;
				} else {
					uni.showToast({ title: res.message, icon: 'none' });
				}
			} catch (error) {
				uni.showToast({ title: '网络请求失败', icon: 'none' });
			} finally {
				this.loading = false;
				this.loding = false;
			}
		}
	},
	// 上拉加载更多
	onReachBottom() {
		if (this.listData.length) {
			this.getList();
		}
	}
};
</script>


<style lang="less" scoped>
.content {
	min-height: auto;
	font-size: medium;
	background-color: #f5f4f4;
	padding: 10rpx 20rpx;
}

.card {
	margin-bottom: 30rpx;
	padding: 0rpx 30rpx;
	border-radius: 0.5rem;
	// box-shadow: 0px 187px 75px rgba(0, 0, 0, 0.01), 0px 105px 63px rgba(0, 0, 0, 0.05), 0px 47px 47px rgba(0, 0, 0, 0.09), 0px 12px 26px rgba(0, 0, 0, 0.1), 0px 0px 0px rgba(0, 0, 0, 0.1);
	background-color: #FFFFFF;
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
	height: 36px;
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

.load-more {
	text-align: center;
	padding: 20rpx;
	color: #999;
	font-size: 28rpx;
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

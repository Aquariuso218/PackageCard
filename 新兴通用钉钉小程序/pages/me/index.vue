<template>
	<view class="container">
		<view class="bg-top bggreen">
			<!-- 个人信息 -->
			<view class="center-box shadow">
				<view class="cu-list menu">
					<view class="cu-bar bg-white margin-top-xs u-border-bottom">
						<view class="action sub-title">
							<text class="text-xl text-bold txtgreen text-shadow">个人信息</text>
							<text class="text-ABC txtgreen">Personal</text>
						</view>
					</view>
					<view class="cu-item" style="padding: 0;">
						<view class="content">
							<text class="cuIcon-myfill txtgreen"></text>
							<text class="text-lg">头像</text>
						</view>
						<view class="action">
							<image style="width: 80rpx; height: 80rpx;" src="/static/images/user.png"></image>
						</view>
					</view>
					<view class="cu-item" style="padding: 0;">
						<view class="content">
							<text class="cuIcon-myfill txtgreen"></text>
							<text class="text-lg">账号</text>
						</view>
						<view class="action">
							<text class="text-grey text-sm">{{ userInfo.Account }}</text>
						</view>
					</view>
					<view class="cu-item" style="padding: 0;">
						<view class="content">
							<text class="cuIcon-myfill txtgreen"></text>
							<text class="text-lg">昵称</text>
						</view>
						<view class="action">
							<text class="text-grey text-sm">{{ userInfo.UserName }}</text>
						</view>
					</view>

					<view class="cu-item" style="padding: 0;">
						<view class='content'>
							<text class="cuIcon-mobilefill txtgreen"></text>
							<text class='text-lg'>联系方式</text>
						</view>
						<view class="action">
							<view class="cu-tag round bggreen light" v-if="userInfo.mobile">{{ userInfo.mobile }}</view>
							<view class="cu-tag round bgred light" v-else>未获取</view>
						</view>
					</view>
					<view class="cu-item" style="padding: 0;">
						<u-button type="error" size="larger" text="退出登录" @click="exit"></u-button>
					</view>
				</view>
			</view>
		</view>
		<wk-wxlogin :zheshow="true" />
	</view>
</template>
<script>
import wkWxlogin from '@/components/wk-wxlogin/wk-wxlogin.vue';
export default {
	components: {
		wkWxlogin
	},
	data() {
		return {
			userInfo: {},
			mobile: "",
			zheshow: true,
		};
	},
	onShow() {
		//加载
		uni.getStorage({
			key: 'userData',
			success: (res) => {
				console.log('123')
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
	},
	onLoad() {

	},
	methods: {
		btns(e) {
			if (e.detail.errMsg === "getPhoneNumber:ok") {
				const { iv, encryptedData } = e.detail;

				wx.login({
					success: (res) => { // 确保 code 赋值后再请求
						if (res.code) {
							const code = res.code;

							// 发送请求到后端
							uni.request({
								url: "http://localhost:8888/v1/WeChat/decryptPhone",
								method: "POST",
								header: { "content-type": "application/json" },
								data: {
									encryptedData,
									iv,
									code: code // 确保这里传入的是获取到的 code
								},
								success: (res) => {
									console.log("后端返回:", JSON.stringify(res));
									if (res.data.data.success) {
										this.mobile = res.data.data.phoneNumber;
									} else {
										uni.showToast({ title: "手机号解密失败", icon: "none" });
									}
								},
								fail: () => {
									uni.showToast({ title: "请求失败，请检查网络", icon: "none" });
								}
							});

						} else {
							console.log("获取 code 失败:", res.errMsg);
							uni.showToast({ title: "获取 code 失败", icon: "none" });
						}
					},
					fail: () => {
						uni.showToast({ title: "wx.login 失败", icon: "none" });
					}
				});
			} else {
				uni.showToast({ title: "用户拒绝授权", icon: "none" });
			}
		},
		exit() {
			// 清除本地存储的用户信息
			// uni.clearStorageSync();
			// 清除本地存储的特定用户信息（只清除 key 为 'userData' 的数据）
			uni.removeStorageSync('userData');
			// 退出到登录界面
			uni.reLaunch({
				url: '/pages/login/login',
			});
		},
	}
}
</script>

<style lang="scss" scoped>
.bggreen {
	background-color: #43C6AC;
}

.bgred {
	background-color: coral;
}

.txtgreen {
	color: #43C6AC;
}

.uni-row {
	// 组件在小程序端display为inline
	// QQ、抖音小程序文档写有 :host，但实测不生效
	// 百度小程序没有 :host
	/* #ifdef MP-TOUTIAO || MP-QQ || MP-BAIDU */
	display: block;
	border-radius: 10rpx;
	/* #endif */
}

.firstrow {
	margin-top: 20rpx;
	height: 60rpx;
	padding-top: 10rpx;
	border-bottom: 2rpx solid rgba(0, 0, 0, 0.3);
}

.uni-col {
	height: 56px;
	border-radius: 5px;
	display: flex;
	// justify-content: center;
	align-items: center;
}

.divRight {
	text-align: right;
	display: flex;
	justify-content: flex-end;
}

.mypadding {
	padding: 20rpx 80rpx;
}

.container {
	width: 750rpx;
	color: #333333;

	.bg-top {
		margin-top: -1rpx;
		width: 750rpx;
		height: 220rpx;
		padding-top: 50rpx;
		border-radius: 0 0 20% 20%;

		.top-box {
			width: 700rpx;
			background-color: #FFFFFF;
			margin: 0 auto;
			border-radius: 20rpx;
			padding: 20rpx 30rpx 0rpx;
			position: relative;

			.qh-pic {
				position: absolute;
				right: 64rpx;
				top: -50rpx;
				border-radius: 12rpx;
			}

			.qh-title {
				width: 100%;
				height: 60rpx;
				line-height: 65rpx;
				padding-right: 190rpx;
			}

			.bxBox {
				position: relative;
				display: flex;
				/* padding: 0 30rpx; */
				min-height: 100rpx;
				/* background-color: #ffffff; */
				/* justify-content: space-between; */
				align-items: center;
				font-size: 30rpx;
				line-height: 1.6em;
				flex: 1;

				.bxImg {
					display: inline-block;
					margin-right: 10rpx;
					width: 1.6em;
					text-align: center;
				}
			}

		}
	}

	.center-box {
		color: #333333;
		width: 700rpx;
		background-color: #FFFFFF;
		margin: 0 auto;
		border-radius: 20rpx;
		padding: 0rpx 30rpx 0rpx;
		position: relative;
		margin-top: 20rpx;
	}
}
</style>

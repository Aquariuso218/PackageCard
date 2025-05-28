<template>
	<view class="content">
		<!-- From Uiverse.io by kamehame-ha -->
		<div class="cards">
			<div class="card blue" @click="topage1()">
				<p class="tip">产品入库</p>
				<p class="second-text">Product Stock In</p>
			</div>
			<div class="card blue" @click="topage2()">
				<p class="tip">销售出库</p>
				<p class="second-text">Sale Stock Out</p>
			</div>
		</div>
	</view>
</template>

<script>
	import * as dd from 'dingtalk-jsapi'; // 引入 dingtalk-jsapi
	import {
		getUserInfo,
		salesInvoice
	} from '@/nxTemp/apis/sunset.js'
	import {
		BASE_URL
	} from '@/env.js'

	export default {
		data() {
			return {
				num: 0, // 初始化 num 变量
				userInfo: null
			};
		},
		onLoad(parms) {
			// 从缓存中获取 userInfo
			const cachedUserInfo = uni.getStorageSync('userInfo');

			// 如果缓存中存在有效的 userInfo，则直接使用
			if (cachedUserInfo && this.isValidUserInfo(cachedUserInfo)) {
				this.userInfo = cachedUserInfo;
				console.log('使用缓存中的 userInfo:', cachedUserInfo);
			} else {
				// 如果缓存为空或无效，才获取新的用户信息
				this.getDingTalkUserInfo();
			}
		},
		onUnload() {},
		methods: {
			async topage1() {
				uni.navigateTo({
					url: '/pages/confirm/index'
				});
			},
			topage2() {
				uni.navigateTo({
					url: '/pages/inquiry/index'
				});
			},

			// 验证 userInfo 是否有效
			isValidUserInfo(userInfo) {
				// 例如，检查关键字段是否存在且不为空
				return userInfo && userInfo.userId && userInfo.name; // 可根据需要添加更多字段
			},

			// 获取钉钉用户信息
			getDingTalkUserInfo() {
				const that = this;
				// 钉钉环境下的登录操作
				dd.ready(function() {
					dd.runtime.permission.requestAuthCode({
						corpId: 'ding802dd01e436de1eca39a90f97fcb1e09', // 从缓存中获取 corpId，需确保已存储
						onSuccess: function(info) {
							const code = info.code; // 获取免登授权码
							console.log("免登授权码:", JSON.stringify(info));
							that.queryLoginInfo(code); // 调用获取用户信息的函数
						},
						onFail: function(err) {
							uni.showToast({
								title: '获取个人信息异常，请稍后重试',
								icon: 'none'
							});
						}
					});
				});
			},

			async queryLoginInfo(code) {
				await getUserInfo({
					code: code
				}).then(res => {
					console.log(JSON.stringify(res))
					if (res.mescode === 0) {
						const userInfo = res.mesdata;
						//存入缓存
						uni.setStorageSync('userInfo', userInfo);
						//更新页面数据
						this.userInfo = userInfo;
						console.log('用户信息获取成功:', userInfo);
					} else {
						console.error('获取用户信息失败:', res.errmsg);
						uni.showToast({
							title: '获取用户信息失败!',
							icon: 'none'
						});
					}
				}).catch(error => {
					console.error('请求失败:', error);
					uni.showToast({
						title: '网络请求失败',
						icon: 'none'
					});
				})
			}
		}
	}
</script>

<style scoped>
	.content {
		padding: 50rpx;
	}

	/* From Uiverse.io by kamehame-ha */
	.cards {
		display: flex;
		flex-direction: column;
		gap: 15px;
	}

	/* 卡片颜色 */
	.cards .red {
		background-color: #f43f5e;
	}

	.cards .blue {
		background-color: #3b82f6;
	}

	.cards .green {
		background-color: #43C6AC;
	}

	/* 卡片基础样式 */
	.cards .card {
		display: flex;
		align-items: center;
		justify-content: center;
		flex-direction: column;
		text-align: center;
		height: 200rpx;
		width: 100%;
		border-radius: 10px;
		color: white;
		cursor: pointer;
		transition: 400ms;
	}

	/* 卡片文字样式 */
	.cards .card p.tip {
		font-size: 1em;
		font-weight: 700;
	}

	.cards .card p.second-text {
		font-size: 0.7em;
	}

	/* 定义点击动画 */
	@keyframes clickAnimation {
		0% {
			transform: scale(1.04);
			background-color: #11998e;
		}

		50% {
			transform: scale(1.04);
		}

		100% {
			transform: scale(1.05);
		}
	}

	/* 点击时触发动画 */
	.cards .card:active {
		animation: clickAnimation 0.2s ease-in-out;
	}
</style>
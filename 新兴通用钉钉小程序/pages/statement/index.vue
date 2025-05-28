<template>
    <view class="content">
        <!-- From Uiverse.io by kamehame-ha -->
        <div class="cards">
            <div class="card blue" @click="topage1()">
                <p class="tip">工序报表</p>
                <p class="second-text">Statement1</p>
            </div>
            <div class="card blue" @click="topage2()">
                <p class="tip">锻坯报表</p>
                <p class="second-text">Statement2</p>
            </div>
        </div>

        <view>
            <u-popup :customStyle="popupStyle" :round="10" :mode="'center'" :show="showPhoneModal">
                <view
                    style="flex: 1; margin-right: 10px; display: flex; justify-content: center; align-items: center; font-weight: bold;">
                    提示
                </view>
                <view style="flex: 1; margin-right: 10px; margin-top: 30rpx;">
                    <u--input v-model="mobile" maxlength="11" placeholder="请输入您的手机号" border="surround" type="number"
                        clearable></u--input>
                </view>
                <view style="display: flex; justify-content: space-between; margin-top: 30rpx;">
                    <u-button slot="confirmButton" text="确定" type="success" shape="circle" @click="sure"></u-button>                   
                    <view style="width:30rpx ;"></view>
                    <u-button slot="confirmButton" text="取消" color="#bdc3c7" shape="circle" @click="back"
                        style="margin-right: 10px;">
                    </u-button>
                </view>
            </u-popup>
            <!-- <u-modal v-if="showPhoneModal" title="提示" show="true">
                <view style="display: flex; justify-content: space-between;">
                    <view style="flex: 1; margin-right: 10px;">
                        <u--input v-model="mobile" maxlength="11" placeholder="请输入您的手机号" border="surround" type="number"
                            clearable></u--input>
                    </view>
                    <view style="display: flex; justify-content: space-between;">
                        <u-button slot="confirmButton" text="取消" type="success" shape="circle" @click="sure"
                            style="margin-right: 10px;"></u-button>
                        <u-button slot="confirmButton" text="确定" type="success" shape="circle" @click="sure"></u-button>
                    </view>
                </view>
            </u-modal> -->
        </view>
    </view>
</template>

<script>
import { SetPhone } from "@/nxTemp/apis/sunset.js"
export default {
    data() {
        return {
            popupStyle: {
                padding: '20rpx',
                width: '70vw',
                height: '20vh',
                backgroundColor: '#ffffff', // 添加背景色
                // borderRadius: '20rpx', // 添加圆角
            },
            showPhoneModal: false, // 控制用户id授权弹窗
            userInfo: [],
            mobile: ''
        };
    },
    onLoad() {
        uni.getStorage({
            key: 'userData',
            success: (res) => {
                this.userInfo = res.data; // 赋值到页面数据
                if (!this.userInfo.mobile && this.userInfo.Account !== "14322") {
                    // 如果用户id不存在，弹出授权窗口
                    this.showPhoneModal = true;
                }
            },
            fail: () => {
                uni.reLaunch({
                    url: '/pages/login/login'
                });
            }
        });
    },
    onUnload() { },
    methods: {
        back(){
            uni.reLaunch({
                    url: '/pages/index/index'
                });
        },
        async sure() {
            if (!this.handleInput(this.mobile)) {
                return;
            }

            const res = await SetPhone({
                Openid: this.userInfo.Openid,
                mobile: this.mobile
            });
            if (res.success) {
                this.userInfo.mobile = this.mobile;
                uni.setStorage({
                    key: 'userData',
                    data: this.userInfo,
                });
                this.showPhoneModal = false;
            } else {
                uni.showToast({
                    title: '手机号绑定失败！',
                    icon: 'none'
                });
            }
        },


        // 手机号正则校验（简单版）
        isPhoneValid(phone) {
            return /^1[3-9]\d{9}$/.test(phone)
        },

        // 输入处理
        handleInput(value) {
            // 限制只能输入数字
            this.phoneNumber = value.replace(/\D/g, '')

            // 实时校验（可根据需求改为失焦校验）
            return this.validatePhone();
        },

        // 校验逻辑
        validatePhone() {
            if (!this.phoneNumber) {
                uni.showToast({
                    title: '手机号不能为空',
                    icon: 'none'
                });
                return false;
            }
            if (!this.isPhoneValid(this.phoneNumber)) {
                uni.showToast({
                    title: '请输入有效手机号',
                    icon: 'none'
                });
                return false;
            }
            return true;
        },
        topage1() {
            // if (this.userInfo.mobile) {
            uni.navigateTo({
                url: '/pages/statement/statement1'
            });
            // } else {
            //     uni.showToast({ title: "手机号未获取", icon: "none" });
            // }
        },
        topage2() {
            // if (this.userInfo.mobile) {
            uni.navigateTo({
                url: '/pages/statement/test'
            });
            // } else {
            //     uni.showToast({ title: "手机号未获取", icon: "none" });
            // }
        },


    },
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
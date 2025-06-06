<template>
	<view>
		<view class="maskbox" v-show="showSelector&&showMask" @click="showSelector=!showSelector"></view>
		<view class="uni-combox" :style="{zIndex:showSelector?999:1}">
			<view v-if="label" class="uni-combox__label" :style="labelStyle">
				<text>{{label}}</text>
			</view>
			<view class="uni-combox__input-box">
				<view class="combox_tags">
					<view class="combox_tag" v-for="(item,index) in selectList" :key="item[keyId]"
						@click="delSelected(index)">
						<view class="tag_name">{{item[keyName]}}</view>
						<icon type="clear" size="16" />
					</view>
				</view>
				<view class="combox_input_view">
					<input class="uni-combox__input" type="text" :placeholder="placeholder" :disabled="isDisabled"
						v-model="inputVal" @input="onInput" @focus="onFocus" @blur="onBlur"
						@click="isDisabled?toggleSelector():''" />
					<icon type="clear" size="16" @tap="clearInputValue" v-if="!isDisabled" />
					<image class="uni-combox__input-arrow" src="./arrow_down.png" style="width: 30rpx;height: 30rpx;"
						@click="toggleSelector"></image>
					<view class="uni-combox__selector" v-if="showSelector">
						<scroll-view scroll-y="true" :style="{maxHeight:selectHeight+'px'}"
							class="uni-combox__selector-scroll">
							<view class="uni-combox__selector-empty" v-if="filterCandidatesLength === 0">
								<text>{{emptyTips}}</text>
							</view>
							<view class="uni-combox__selector-item" v-for="(item,index) in filterCandidates"
								:key="item[keyId]" @click="onSelectorClick(index)"
								:style="isCheck(item[keyId])?'color:'+selectColor+';background-color:'+selectBgColor+';':'color:'+color+';'">
								<text>{{item[keyName]}}</text>
							</view>
						</scroll-view>
					</view>
				</view>
			</view>
		</view>
	</view>
</template>

<script>
	/**
	 * Combox 组合输入框
	 * @description 组合输入框一般用于既可以输入也可以选择的场景
	 * @property {String} unikey 一个页面同时使用多个组件时用不同值区分
	 * @property {String} label 左侧文字
	 * @property {String} labelWidth 左侧内容宽度
	 * @property {String} placeholder 输入框占位符
	 * @property {Array} candidates 候选项列表
	 * @property {String} emptyTips 筛选结果为空时显示的文字
	 * @property {String} value 单选时组合框的初始值
	 * @property {Array} initValue 多选时组合框的初始值(下标集合)
	 * @property {String} keyName json数组显示的字段值
	 * @property {String} keyId json数组返回的字段
	 * @property {String} showMask 展示下拉框是否显示遮罩
	 * @property {Boolean} isJSON 是否是json数组，默认不是
	 * @property {Boolean} isDisabled 是否是禁用输入，默认不禁用
	 * @property {Boolean} isCheckBox 是否是多选，默认不是，多选时不能输入查询
	 * @property {Number} maxNum 多选最多选择个数
	 * @property {String} color 默认字体颜色，默认#000000
	 * @property {String} selectColor 选中字体颜色，默认#0081ff
	 * @property {String} selectBgColor 选中背景颜色，默认#e8e8e8
	 * @property {String} selectHeight 下拉框最大高度，默认200px
	 */
	export default {
		name: 'comboxSearch',
		props: {
			unikey: {
				type: Number,
				default: 0
			},
			label: {
				type: String,
				default: ''
			},
			labelWidth: {
				type: String,
				default: 'auto'
			},
			placeholder: {
				type: String,
				default: ''
			},
			candidates: {
				type: Array,
				default () {
					return []
				}
			},
			emptyTips: {
				type: String,
				default: '无匹配项'
			},
			value: {
				type: String,
				default: ''
			},
			initValue: {
				type: Array,
				default: null
			},
			keyName: {
				type: String,
				default: 'boxname'
			},
			keyId: {
				type: String,
				default: 'boxsort'
			},
			isJSON: {
				type: Boolean,
				default: false
			},
			isDisabled: {
				type: Boolean,
				default: false
			},
			showMask: {
				type: Boolean,
				default: true
			},
			isCheckBox: {
				type: Boolean,
				default: false
			},
			maxNum: {
				type: Number,
				default: -1
			},
			color: {
				default: "#000000",
				type: String
			},
			selectColor: {
				default: "#0081ff",
				type: String
			},
			selectBgColor: {
				default: "#e8e8e8",
				type: String
			},
			selectHeight: {
				type: Number,
				default: 200
			}
		},
		data() {
			return {
				showSelector: false,
				inputVal: '',
				selectList: [],
				arrays: [],
				gid: `sm-org-dropDown-${(new Date()).getTime()}${Math.random()}`
			}
		},
		computed: {
			labelStyle() {
				if (this.labelWidth === 'auto') {
					return {}
				}
				return {
					width: this.labelWidth
				}
			},
			filterCandidates() {
				if (!this.isDisabled) {
					return this["candidates" + this.unikey].filter((item) => {
						return item[this.keyName].indexOf(this.inputVal) > -1
					})
				} else {
					return this["candidates" + this.unikey];
				}
			},
			filterCandidatesLength() {
				return this.filterCandidates.length
			}
		},
		created() {
			this["candidates" + this.unikey] = JSON.parse(JSON.stringify(this.candidates));
			this.candidates.forEach((item, index) => {
				if (this.isJSON) {
					if (this.keyId == "boxsort") {
						this["candidates" + this.unikey][index].boxsort = index;
					}
				} else {
					let a = {
						boxsort: index,
						boxname: item
					};
					this["candidates" + this.unikey][index] = a;
				}
			});
			if (this.initValue != null) {
				this.initValue.forEach(v => {
					this["candidates" + this.unikey].forEach(c => {
						if (v == c[this.keyId]) {
							this.selectList.push(c);
						}
					})
				})
			}
			//在created的时候，给子组件放置一个监听，这个时候只是监听器被建立，此时这段代码不会生效
			//uni.$on接收一个sm-org-dropDown-show的广播
			uni.$on('sm-org-dropDown-show', (targetId) => {
				//接收广播，当该组件处于下拉框被打开的状态，并且跟下一个被点击的下拉框的gid不同时，将该组件的下拉框关闭，产生一个互斥效果。
				if (this.showSelector && this.gid != targetId) {
					this.showSelector = false
				}
			})
		},
		//当子组件销毁的时候，将建立的监听销毁，这里不会影响代码效果，但是在多次反复引用组件时，会在根节点定义越来越多的监听，长此以往影响性能，所以在不用的时候及时销毁，养成好习惯
		beforeDestroy() {
			uni.$off('sm-org-dropDown-show')
		},
		watch: {
			value: {
				handler(newVal) {
					this.inputVal = newVal
				},
				immediate: true
			}
		},
		methods: {
			toggleSelector() {
				this.showSelector = !this.showSelector
				if (this.showSelector) {
					uni.$emit('sm-org-dropDown-show', this.gid)
				}
			},
			onFocus() {
				this.showSelector = true;
				uni.$emit('sm-org-dropDown-show', this.gid)
			},
			onBlur() {

			},
			onSelectorClick(index) {
				if (this.isCheckBox) {
					let flag = this.selectList.
					findIndex(item => item[this.keyId] == this.filterCandidates[index][this.keyId]);
					if (flag != -1) {
						this.selectList.splice(flag, 1);
					} else {
						if (this.selectList.length == this.maxNum) {
							return;
						}
						this.selectList.push(this.filterCandidates[index]);
					}
					this.$emit('getValue', this.selectValue());
				} else {
					this.showSelector = false
					if (this.isJSON) {
						this.$emit('getValue', this.filterCandidates[index][this.keyId]);
					} else {
						this.$emit('getValue', this.filterCandidates[index][this.keyName]);
					}
					this.inputVal = this.filterCandidates[index][this.keyName];
				}
			},
			selectValue() {
				let arr = [];
				if (this.isJSON) {
					this.selectList.forEach(v => {
						arr.push(v[this.keyId]);
					})
				} else {
					this.selectList.forEach(v => {
						arr.push(v[this.keyName]);
					})
				}
				return arr;
			},
			delSelected(index) {
				this.selectList.splice(index, 1);
				this.$emit('getValue', this.selectValue());
			},
			onInput() {
				setTimeout(() => {
					this.$emit('input', this.inputVal)
				})
			},
			clearInputValue() {
				this.inputVal = "";
				this.showSelector = false;
			},
			isCheck(keyId) {
				return this.selectList.findIndex(v => v[this.keyId] == keyId) != -1;
			}
		}
	}
</script>

<style scoped>
	.uni-combox {
		/* #ifndef APP-NVUE */
		display: flex;
		/* #endif */
		min-height: 40px;
		flex-direction: row;
		align-items: center;
		position: relative;
	}

	.uni-combox__label {
		font-size: 16px;
		line-height: 22px;
		padding-right: 10px;
		color: #999999;
	}

	.uni-combox__input-box {
		/* #ifndef APP-NVUE */
		display: flex;
		/* #endif */
		flex: 1;
		flex-direction: column;
	}

	.combox_tags {
		display: flex;
		justify-content: flex-start;
		flex-wrap: wrap;
	}

	.combox_tag {
		padding: 10rpx 15rpx;
		background-color: #F0F2F5;
		color: #94979D;
		margin-right: 10rpx;
		display: flex;
		justify-content: flex-start;
		align-items: center;
		margin-bottom: 10rpx;
	}

	.combox_tag .tag_name {
		margin-right: 10rpx;
	}

	.combox_input_view {
		position: relative;
		/* #ifndef APP-NVUE */
		display: flex;
		/* #endif */
		flex: 1;
		flex-direction: row;
		align-items: center;
		min-width: 100%;
	}

	.uni-combox__input {
		flex: 1;
		font-size: 16px;
		height: 22px;
		line-height: 22px;
	}

	.uni-combox__input-arrow {
		padding: 10px;
	}

	.uni-combox__selector {
		box-sizing: border-box;
		position: absolute;
		top: 42px;
		left: 0;
		width: 100%;
		background-color: #FFFFFF;
		border-radius: 6px;
		box-shadow: #DDDDDD 4px 4px 8px, #DDDDDD -4px -4px 8px;
		z-index: 2;
	}

	.uni-combox__selector-scroll {
		box-sizing: border-box;
	}

	.uni-combox__selector::before {
		content: '';
		position: absolute;
		width: 0;
		height: 0;
		border-bottom: solid 6px #FFFFFF;
		border-right: solid 6px transparent;
		border-left: solid 6px transparent;
		left: 50%;
		top: -6px;
		margin-left: -6px;
	}

	.uni-combox__selector-empty,
	.uni-combox__selector-item {
		/* #ifdef APP-NVUE */
		display: flex;
		/* #endif */
		line-height: 36px;
		font-size: 14px;
		text-align: center;
		border-bottom: solid 1px #DDDDDD;
		/* margin: 0px 10px; */
	}

	.uni-combox__selector-empty:last-child,
	.uni-combox__selector-item:last-child {
		border-bottom: none;
	}

	.maskbox {
		position: fixed;
		top: 0;
		left: 0;
		width: 100vw;
		height: 100vh;
		z-index: 99;
	}
</style>

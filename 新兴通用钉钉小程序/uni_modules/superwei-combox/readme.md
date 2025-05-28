## 基本用法
在 ``template`` 中使用组件
```html
<template>
	<view class="content">
		<span class="title">非JSON数组模式</span>
		<superwei-combox :candidates="candidates" placeholder="请选择或输入" v-model="inputValue" @input="input"
			@select="select"></superwei-combox>

		<span class="title">JSON数组模式</span>
		<superwei-combox :candidates="candidates_json" :isJSON="true" keyName="name" placeholder="请选择或输入"
			v-model="inputValue_json" @input="input_json" @select="select_json"></superwei-combox>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				inputValue: '',
				candidates: ['选项一', '选项二', '选项三', '选项四', '选项五', '选项六', '...'],

				inputValue_json: '',
				candidates_json: [{
					id: '1',
					name: '选项一'
				}, {
					id: '2',
					name: '选项二',
					disabled: true // 单独设置disabled后即可禁用该选项
				}, {
					id: '3',
					name: '选项三'
				}, {
					id: '4',
					name: '选项四'
				}, {
					id: '5',
					name: '选项五',
					disabled: true // 单独设置disabled后即可禁用该选项
				}, {
					id: '6',
					name: '...'
				}]
			}
		},
		methods: {
			input(e) {
				console.log(e) // 选项一
			},
			select(e) {
				console.log(e) // 选项一
			},


			input_json(e) {
				console.log(e) // 选项一
			},
			select_json(e) {
				console.log(e) // {id: '1',name: '选项一'}
			}
		}
	}
</script>

<style>
	.content {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.title {
		margin-top: 20px;
	}
</style>
```

## API

### Combox Props

|属性名		|类型			|默认值		|说明								|
|:-:		|:-:			|:-:		|:-:								|
|label		|String			|-			|标签文字							|
|value		|String			|-			|combox的值							|
|labelWidth	|String			|auto		|标签宽度，有单位字符串，如:'100px'	|
|placeholder|String			|-			|输入框占位符						|
|candidates	|Array/String	|[]			|候选字段							|
|emptyTips	|String			|无匹配项	|无匹配项时的提示语					|
|selectedBackground	|String			|#f5f7fa	|选中项背景颜色					|
|selectedColor	|String			|#409eff	|选中项文字颜色					|
|isJSON	|Boolean			|false	|候选字段是否是json数组					|
|keyName	|String			|-	|json数组显示的字段值					|
|disabledColor	|String			|#ababac	|禁用项文字颜色					|
|isAllowCreate	|Boolean			|true	|是否允许用户创建新条目					|

### Combox Events

|事件称名	|说明					|返回值												|
|:-:		|:-:					|:-:													|
|@input	|combox输入事件	|返回combox输入值|
|@select|combox选择事件	|返回combox选项值|

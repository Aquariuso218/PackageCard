<template>
	<view class="container">
	  <movable-area class="movable-area">
		<movable-view class="movable-view" 
					  direction="all" 
					  :scale="true" scale-min="1" scale-max="2">
		  <view class="content">text</view>
		</movable-view>
	  </movable-area>
	</view>
  </template>
  
  <script>
  export default {
	data() {
	  return {
		scale: 1, // 默认缩放比例
		startScale: 1,
		startDistance: 0,
		startTouches: [],
		movableX: 0,
		movableY: 0,
		minScale: 1, // 最小缩放比例（即初始大小）
		maxScale: 1, // 最大缩放比例
	  };
	},
	methods: {
	  onTouchStart(event) {
		if (event.touches.length === 2) {
		  this.startTouches = event.touches;
		  this.startDistance = this.getDistance(event.touches[0], event.touches[1]);
		  this.startScale = this.scale;
		}
	  },
	  onTouchMove(event) {
		if (event.touches.length === 2) {
		  const newDistance = this.getDistance(event.touches[0], event.touches[1]);
		  const scaleChange = newDistance / this.startDistance;
		  let newScale = this.startScale * scaleChange;
  
		  // 限制缩放范围
		  if (newScale < this.minScale) {
			newScale = this.minScale;  // 最小缩放比例
		  }
		  if (newScale > this.maxScale) {
			newScale = this.maxScale;  // 最大缩放比例
		  }
  
		  this.scale = newScale;
		}
  
		// 移动
		if (event.touches.length === 1) {
		  const touch = event.touches[0];
		  this.movableX = touch.clientX;
		  this.movableY = touch.clientY;
		}
	  },
	  onTouchEnd(event) {
		if (event.touches.length < 2) {
		  this.startTouches = [];
		}
	  },
	  // 计算两点间的距离
	  getDistance(touch1, touch2) {
		const dx = touch2.clientX - touch1.clientX;
		const dy = touch2.clientY - touch1.clientY;
		return Math.sqrt(dx * dx + dy * dy);
	  },
	},
  };
  </script>
  
  <style scoped>
  .container {
	width: 100%;
	height: 100%;
	display: flex;
	justify-content: center;
	align-items: center;
	background-color: #f0f0f0;
  }
  
  .movable-area {
	width: 100%;
	height: 100vh;
	background-color: aqua;
	border: 1px solid #ccc;
  }
  
  .movable-view {
	width: 100%;
	height: 100vh;
	background-color: #4caf50;
	color: white;
	text-align: center;
	line-height: 100px;
	border-radius: 10px;
	user-select: none;
  }
  
  .content {
	width: 100%;
	height: 100%;
	background-color: blueviolet;
	transform-origin: center;
  }
  </style>
  
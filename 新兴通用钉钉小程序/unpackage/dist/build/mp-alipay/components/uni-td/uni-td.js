;my.defineComponent || (my.defineComponent = Component);(my["webpackJsonp"]=my["webpackJsonp"]||[]).push([["components/uni-td/uni-td"],{"3067a":function(t,n,e){"use strict";e.d(n,"b",(function(){return r})),e.d(n,"c",(function(){return i})),e.d(n,"a",(function(){}));var r=function(){var t=this.$createElement;this._self._c},i=[]},"3e1f":function(t,n,e){"use strict";e.r(n);var r=e("3067a"),i=e("46fa");for(var u in i)["default"].indexOf(u)<0&&function(t){e.d(n,t,(function(){return i[t]}))}(u);e("5854");var a=e("828b"),o=Object(a["a"])(i["default"],r["b"],r["c"],!1,null,null,null,!1,r["a"],void 0);n["default"]=o.exports},"43d2":function(t,n,e){"use strict";Object.defineProperty(n,"__esModule",{value:!0}),n.default=void 0;var r={name:"uniTd",options:{virtualHost:!0},props:{width:{type:[String,Number],default:""},align:{type:String,default:"left"},rowspan:{type:[Number,String],default:1},colspan:{type:[Number,String],default:1}},data:function(){return{border:!1}},created:function(){this.root=this.getTable(),this.border=this.root.border},methods:{getTable:function(){var t=this.$parent,n=t.$options.name;while("uniTable"!==n){if(t=t.$parent,!t)return!1;n=t.$options.name}return t},handleClick:function(){this.$emit("click")}}};n.default=r},"46fa":function(t,n,e){"use strict";e.r(n);var r=e("43d2"),i=e.n(r);for(var u in r)["default"].indexOf(u)<0&&function(t){e.d(n,t,(function(){return r[t]}))}(u);n["default"]=i.a},5854:function(t,n,e){"use strict";var r=e("e20c"),i=e.n(r);i.a},e20c:function(t,n,e){}}]);
;(my["webpackJsonp"] = my["webpackJsonp"] || []).push([
    'components/uni-td/uni-td-create-component',
    {
        'components/uni-td/uni-td-create-component':(function(module, exports, __webpack_require__){
            __webpack_require__('6861')['createComponent'](__webpack_require__("3e1f"))
        })
    },
    [['components/uni-td/uni-td-create-component']]
]);

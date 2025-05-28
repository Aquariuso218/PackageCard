;my.defineComponent || (my.defineComponent = Component);(my["webpackJsonp"]=my["webpackJsonp"]||[]).push([["uview-ui/components/u-status-bar/u-status-bar"],{"101c":function(t,e,n){},"69f6":function(t,e,n){"use strict";(function(t){var u=n("47a9");Object.defineProperty(e,"__esModule",{value:!0}),e.default=void 0;var a=u(n("e5ac")),i={name:"u-status-bar",mixins:[t.$u.mpMixin,t.$u.mixin,a.default],data:function(){return{}},computed:{style:function(){var e={};return e.height=t.$u.addUnit(t.$u.sys().statusBarHeight,"px"),e.backgroundColor=this.bgColor,t.$u.deepMerge(e,t.$u.addStyle(this.customStyle))}}};e.default=i}).call(this,n("6861")["default"])},ac7e:function(t,e,n){"use strict";n.r(e);var u=n("69f6"),a=n.n(u);for(var i in u)["default"].indexOf(i)<0&&function(t){n.d(e,t,(function(){return u[t]}))}(i);e["default"]=a.a},bf27:function(t,e,n){"use strict";n.r(e);var u=n("fe7d"),a=n("ac7e");for(var i in a)["default"].indexOf(i)<0&&function(t){n.d(e,t,(function(){return a[t]}))}(i);n("f6b4");var r=n("828b"),s=Object(r["a"])(a["default"],u["b"],u["c"],!1,null,"303afcbe",null,!1,u["a"],void 0);e["default"]=s.exports},f6b4:function(t,e,n){"use strict";var u=n("101c"),a=n.n(u);a.a},fe7d:function(t,e,n){"use strict";n.d(e,"b",(function(){return u})),n.d(e,"c",(function(){return a})),n.d(e,"a",(function(){}));var u=function(){var t=this.$createElement,e=(this._self._c,this.__get_style([this.style]));this.$mp.data=Object.assign({},{$root:{s0:e}})},a=[]}}]);
;(my["webpackJsonp"] = my["webpackJsonp"] || []).push([
    'uview-ui/components/u-status-bar/u-status-bar-create-component',
    {
        'uview-ui/components/u-status-bar/u-status-bar-create-component':(function(module, exports, __webpack_require__){
            __webpack_require__('6861')['createComponent'](__webpack_require__("bf27"))
        })
    },
    [['uview-ui/components/u-status-bar/u-status-bar-create-component']]
]);

tpr = new Vue({
    el: '#work',
    data: {
        message: '',
        type: '',
        ms: false,
        dis_save: true,
        dis_update: false,
        id: '',
        img_src: '',
        patInfo: {
            PatNO: '',
            PatID: '',
            PatName: '',
            Sex: '',
            Birthday: ''
        },
        bd: '',
        ed: '',
        date: '',
        time: '',
        tprData: {
            PatNO: '',
            MonitorDate: '',
            MonitorTime: '',
            Temperature: '',
            Weight: '',
            Pulse: '',
            Breath: '',
            DBP: '',
            SBP: '',
            SPO2: ''
        },
        chartData: {
            columns: ['日期', '體溫', '體重', '心跳', '呼吸', '舒張壓', '收縮壓', 'SpO2'],
            rows: []
        },
        v_date: '',
        chartEvents: {

        },
        dataZoom: {
            type: 'slider',
            start: 0,
            end: 30
        },
        loading: false,
        dataEmpty: true,
        show: false,
        PerPage: 10,
        CurrentIndex: 1
    },
    created: function () {
        var tpr = this;

        this.chartEvents = {
            click: function (e) {
                console.log(tpr.chartData.rows[e.dataIndex]);
                var item = tpr.chartData.rows[e.dataIndex];

                tpr.date = item.日期.substr(0, 10);
                tpr.time = item.日期.substr(11, 5);

                tpr.tprData.MonitorDate = item.日期.substr(0, 4) + item.日期.substr(5, 2) + item.日期.substr(8, 2);
                tpr.tprData.MonitorTime = item.日期.substr(11, 2) + item.日期.substr(14, 2);

                tpr.tprData.Temperature = item.體溫;
                tpr.tprData.Weight = item.體重;
                tpr.tprData.Pulse = item.心跳;
                tpr.tprData.Breath = item.呼吸;
                tpr.tprData.DBP = item.舒張壓;
                tpr.tprData.SBP = item.收縮壓;
                tpr.tprData.SPO2 = item.SpO2;
                tpr.dis_save = false;
                tpr.dis_update = true;
            }
        }
    },
    computed: {
        detailTotalPage: {
            get: function () {
                var tpr = this;
                return Math.ceil(tpr.chartData.rows.length / tpr.PerPage);
            }
        },
        detailDataRows: {
            get: function () {
                var tpr = this;
                if (tpr.chartData.rows.length > 0) {
                    let a = tpr.chartData.rows.map(function (item, index, array) {
                        let low = (tpr.CurrentIndex - 1) * tpr.PerPage;
                        let high = (tpr.CurrentIndex) * tpr.PerPage - 1;
                        if (index >= low && index <= high) {
                            return item;
                        }
                    });
                    a = a.filter(function (item) {
                        return item != undefined;
                    });
                    return a;
                }
                else {
                    return [{ "日期": "", "體溫": "", "體重": "", "心跳": "", "舒張壓": "", "收縮壓": "", "SpO2": "", "Breath": "" }];
                }
            }
        }
    },
    watch: {
        id: function (val, old) {
            if (val != old) {
                tpr.clear();
            }
        }
    },
    methods: {
        timeFormate: function (date, fmt) {
            if (/(y+)/.test(fmt)) {
                fmt = fmt.replace(RegExp.$1, (date.getFullYear() + '').substr(4 - RegExp.$1.length));
            }
            let o = {
                'M+': date.getMonth() + 1,
                'd+': date.getDate(),
                'h+': date.getHours(),
                'm+': date.getMinutes(),
                's+': date.getSeconds()
            };
            for (let k in o) {
                if (new RegExp(`(${k})`).test(fmt)) {
                    let str = o[k] + '';
                    fmt = fmt.replace(RegExp.$1, (RegExp.$1.length === 1) ? str : tpr.padLeftZero(str));
                }
            }
            return fmt;
        },

        padLeftZero: function (str) {
            return ('00' + str).substr(str.length);
        },

        addDays: function (date, days) {
            date.setDate(date.getDate() + days);
            return date;
        },

        limit: function (l_id) {
            if (l_id - this.id.length < 0) {
                this.id = this.id.substr(0, l_id);
            }
        },

        getPatientInfo: function () {
            var tpr = this;
            $.ajax({
                method: "POST",
                url: "InpatientTPRWS.asmx/GetPatientData",
                data: '{"patNO":"' + tpr.id + '"}',
                contentType: "application/json;charset=utf-8;",
                dataType: "json", //回傳之格式                
                success: function (response, textStatus) {
                    if (textStatus === 'success') {
                        tpr.patInfo = response.d;
                        tpr.bd = tpr.addDays(new Date(), -6);
                        tpr.ed = new Date();
                        tpr.date = tpr.timeFormate(new Date(), 'yyyy-MM-dd');
                        tpr.time = tpr.timeFormate(new Date(), 'hh:mm');
                    }
                },
                error: function (xhr, ajaxOptions, thrownError) {

                },
                complete: function (XMLHttpRequest) {
                    if (tpr.patInfo.Sex == 'M') {
                        tpr.img_src = 'man.jpg';
                    } else if (tpr.patInfo.Sex == 'F') {
                        tpr.img_src = 'woman.jpg';
                    } else {
                        tpr.img_src = '';
                    }
                    tpr.vcharts();
                }
            });
        },

        nextweek: function (days) {
            var tpr = this;
            tpr.addDays(tpr.bd, days);
            tpr.addDays(tpr.ed, days);
            tpr.vcharts();
        },

        vcharts: function () {
            var tpr = this;

            var begDate = tpr.timeFormate(tpr.bd, 'yyyyMMdd');
            var endDate = tpr.timeFormate(tpr.ed, 'yyyyMMdd');

            var tdata = '{"patNO":"' + tpr.id + '","begDate":"' + begDate + '","endDate":"' + endDate + '"}';
            tpr.loading = true;
            tpr.dataEmpty = true;

            $.ajax({
                method: "POST",
                url: "InpatientTPRWS.asmx/GetTPRs",
                data: tdata,
                contentType: "application/json;charset=utf-8;",
                dataType: "json", //回傳之格式                
                success: function (response, textStatus) {
                    if (textStatus === "success") {
                        var tprs = response.d;
                        tpr.chartData.rows = [];
                        tpr.dataEmpty = tprs.length <= 0;
                        tprs.forEach(function (item, index, array) {
                            tpr.chartData.rows.push({
                                '日期': (item.MonitorDate.substr(0, 4) + '-' + item.MonitorDate.substr(4, 2) + '-' + item.MonitorDate.substr(6, 2)) + " " +
                                    (item.MonitorTime.substr(0, 2) + ':' + item.MonitorTime.substr(2, 2)),
                                '體溫': item.Temperature,
                                '體重': item.Weight,
                                '心跳': item.Pulse,
                                '呼吸': item.Breath,
                                '舒張壓': item.DBP,
                                '收縮壓': item.SBP,
                                'SpO2': item.SPO2
                            });
                        });
                        tpr.loading = false;
                        tpr.v_date = begDate + ' ~ ' + endDate;
                        tpr.show = true;
                    }
                },
                error: function (reponse) {
                    console.log(response);
                    tpr.loading = false;
                },
                complete: function () {

                }
            });
        },

        newdata:function(){
            tpr.date = tpr.timeFormate(new Date(), 'yyyy-MM-dd');
            tpr.time = tpr.timeFormate(new Date(), 'hh:mm');
            tpr.clearInput();
            tpr.dis_save = true;
            tpr.dis_update = false;
        },

        insertPatientInfo: function () {
            var tpr = this;

            tpr.tprData.PatNO = tpr.id;
            tpr.tprData.MonitorDate = tpr.timeFormate(tpr.ed, 'yyyyMMdd');
            tpr.tprData.MonitorTime = tpr.timeFormate(tpr.ed, 'hhmm');
            var tdata = JSON.stringify(tpr.tprData);
            tdata = '{"tprData":' + tdata + '}';

            $.ajax({
                method: "POST",
                url: "InpatientTPRWS.asmx/SaveTPR",
                data: tdata,
                contentType: "application/json;charset=utf-8;",
                dataType: "json", //回傳之格式                
                success: function (response) {

                },
                error: function (xhr, ajaxOptions, thrownError) {

                },
                complete: function (XMLHttpRequest) {
                    if (XMLHttpRequest.status === 200) {
                        tpr.clearInput();
                        tpr.showMessage("儲存成功!!", 'alert-success');
                        tpr.vcharts();
                    } else {
                        tpr.showMessage("請確認資料是否正確!!", 'alert-danger');
                    }
                }
            });
        },

        update: function () {
            var tpr = this;

            tpr.tprData.PatNO = tpr.id;

            var tdata = JSON.stringify(tpr.tprData);
            tdata = '{"tprData":' + tdata + '}';

            $.ajax({
                method: "POST",
                url: "InpatientTPRWS.asmx/UpdateTPR",
                data: tdata,
                contentType: "application/json;charset=utf-8;",
                dataType: "json", //回傳之格式                
                success: function (response) {
                    tpr.vcharts();
                    tpr.clearInput();
                },
                error: function (xhr, ajaxOptions, thrownError) {

                },
                complete: function (XMLHttpRequest) {
                    if (XMLHttpRequest.status === 200) {
                        tpr.clearInput();
                        tpr.showMessage("更新成功!!", 'alert-success');
                        tpr.vcharts();
                        tpr.date = tpr.timeFormate(new Date(), 'yyyy-MM-dd');
                        tpr.time = tpr.timeFormate(new Date(), 'hh:mm');
                        tpr.dis_save = true;
                        tpr.dis_update = false;
                    } else {
                        tpr.showMessage("請確認資料是否正確!!", 'alert-danger');
                    }
                }
            });
        },

        del: function () {
            if (confirm('是否要刪除這筆資料?')) {
                var tpr = this;

                tpr.tprData.PatNO = tpr.id;

                var tdata = JSON.stringify(tpr.tprData);
                tdata = '{"tprData":' + tdata + '}';

                $.ajax({
                    method: "POST",
                    url: "InpatientTPRWS.asmx/DeleteTPR",
                    data: tdata,
                    contentType: "application/json;charset=utf-8;",
                    dataType: "json", //回傳之格式                
                    success: function (response) {
                        tpr.vcharts();
                        tpr.clearInput();
                    },
                    error: function (xhr, ajaxOptions, thrownError) {

                    },
                    complete: function (XMLHttpRequest) {
                        if (XMLHttpRequest.status === 200) {
                            tpr.clearInput();
                            tpr.showMessage("刪除成功!!", 'alert-success');
                            tpr.vcharts();
                            tpr.date = tpr.timeFormate(new Date(), 'yyyy-MM-dd');
                            tpr.time = tpr.timeFormate(new Date(), 'hh:mm');
                            tpr.dis_save = true;
                            tpr.dis_update = false;
                        } else {
                            tpr.showMessage("刪除失敗!!", 'alert-danger');
                        }
                    }
                });
            }
        },

        clearInput: function () {
            var tpr = this;
            tpr.tprData.PatNO = '';
            tpr.tprData.MonitorDate = '';
            tpr.tprData.MonitorTime = '';
            tpr.tprData.Temperature = '';
            tpr.tprData.Weight = '';
            tpr.tprData.Pulse = '';
            tpr.tprData.Breath = '';
            tpr.tprData.DBP = '';
            tpr.tprData.SBP = '';
            tpr.tprData.SPO2 = '';
            tpr.CurrentIndex = 1;
        },

        clear: function () {
            var tpr = this;
            tpr.img_src = '';
            tpr.patInfo.PatNO = '';
            tpr.patInfo.PatID = '';
            tpr.patInfo.PatName = '';
            tpr.patInfo.Sex = '';
            tpr.patInfo.Birthday = '';
            tpr.tprData.PatNO = '';
            tpr.tprData.MonitorDate = '';
            tpr.tprData.MonitorTime = '';
            tpr.tprData.Temperature = '';
            tpr.tprData.Weight = '';
            tpr.tprData.Pulse = '';
            tpr.tprData.Breath = '';
            tpr.tprData.DBP = '';
            tpr.tprData.SBP = '';
            tpr.tprData.SPO2 = '';
            tpr.chartData.rows = [];
            tpr.v_date = '';
            tpr.dataEmpty = true;
            tpr.show = false;
            tpr.CurrentIndex = 1;
        },

        showMessage: function (msg, type) {
            var tpr = this;
            tpr.ms = true;
            tpr.type = type;
            tpr.message = msg;
            if (tpr.ms === true) {
                setTimeout(
                    function () {
                        tpr.ms = false;
                    }, 2000);
            }
        },

        pages: function (pageIdx) {
            var pagesIdx = [];
            if (pageIdx <= 0) {
                return pagesIdx;
            }
            for (i = 2; i > 0; i--) {
                if (pageIdx - i <= 0) {
                    continue;
                }
                pagesIdx.push(pageIdx - i);
            }
            pagesIdx.push(pageIdx);
            for (i = 1; i <= 2; i++) {
                if (pageIdx + i > this.detailTotalPage) {
                    break;
                }
                pagesIdx.push(pageIdx + i);
            }
            return pagesIdx;
        }
    }
})

/// 隨機產出顏色，透明度 100%
function RandomColor_Rgba_transparent_0() {
    return 'rgba(0,0,0,0)';
}

/// 隨機產出顏色，透明度 60%
function RandomColor_Rgba_transparent_0_4() {
    let r = Math.floor(Math.random() * 255); // Random between 0-255
    let g = Math.floor(Math.random() * 255); // Random between 0-255
    let b = Math.floor(Math.random() * 255); // Random between 0-255
    return 'rgba(' + r + ',' + g + ',' + b + ',0.4' + ')'; // Collect all to a string
}

/// 隨機產出顏色，透明度 40%
function RandomColor_Rgba_transparent_0_6() {
    let r = Math.floor(Math.random() * 255); // Random between 0-255
    let g = Math.floor(Math.random() * 255); // Random between 0-255
    let b = Math.floor(Math.random() * 255); // Random between 0-255
    return 'rgba(' + r + ',' + g + ',' + b + ',0.6' + ')'; // Collect all to a string
}

function GetLabelArray() {
    return ['力量', '敏捷', '體力', '智慧', '靈巧', '幸運'];
}

// 重設畫布的方法
function ResetCanvas(id) {
    // 清空 chart container 的內容
    let chart_container = $(".chart-container");
    if (chart_container != null)
        chart_container.empty();

    // 加入新的畫布
    chart_container.append('<canvas id="' + id + '"></canvas>');
}

function RandomNickname() {
    let nameArray = ['無法顯示人物名稱', '銅鑼灣扛報紙', '汗味戰警', '左青龍右胖虎', '靈車甩尾棺材飛',
        '媽你看爸在飛', '怕滑落地', '頭蚊子叮', '紅衣小男孩', '九頭龍閃到腰',
        '善解人衣', '剛好五個字', '打死我也不說', '媽媽來了快關電腦', '怪很強我先走',
        '差一元買多多', '說好不打臉', '不要問 你會怕', '拉鍊裡的猛獸',
        '你看不見我', '轉角遇到怪', '放個屁就', '你點到我了', '抱歉沒點補',
        '網路連線中斷', '拿佛珠砸耶穌', '你已經死了', '綠油精點眼睛', '屎亡筆記本',
        '關完大哥關二哥', '放屁給你聞', '你在大聲什麼啦', '蒙奇D能兒', '名子剛好七個字',
        '苗栗小五郎', 'AI人工智障', '放屁給你聞', '蒙其D罩杯',
        '老爺不可以', '夫人會生氣', '憂鬱肉燥飯', '被冰塊燙傷',
        '麻辣白開水', '涼麵趁熱吃']

    let nameIndex = Math.floor(Math.random() * (nameArray.length));
    return nameArray[nameIndex];
}

function RandomJob() {
    let jobArray = ['法師', '盜賊', '劍士', '弓箭手', '商人', '祭司'];
    let jobIndex = Math.floor(Math.random() * (jobArray.length));
    return jobArray[jobIndex];
}
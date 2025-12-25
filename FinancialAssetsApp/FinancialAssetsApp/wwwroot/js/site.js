// Function for safe Chart creation
const charts = {}; // Global object for storing charts

// Creating charts
function createChart(canvasId, config) {
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    // If there's already a Chart on this canvas — destroy it
    if (charts[canvasId]) {
        charts[canvasId].destroy();
    }

    charts[canvasId] = new Chart(ctx, config);
}   //
// Function for safe fetch + json     // Common functions
async function fetchJson(url) {
    try {
        const res = await fetch(url);
        if (!res.ok) throw new Error(res.statusText);
        const json = await res.json();
        return json;
    } catch (err) {
        console.error(`Error fetching ${url}:`, err);
        return null;
    }
}            //
// Change comma to dot                //
document.addEventListener("DOMContentLoaded", () => {
    document.querySelectorAll('input[name="Price"], input[name="AmountCrypto"]').forEach(input => {
        input.addEventListener("input", () => {
            input.value = input.value.replace(",", ".");
        });
    });
});               //

// Pie chart by RUB tickers
fetchJson('/Stocks/GetChartT').then(data => {
    if (!data) return;
    createChart('TickerPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});

// Pie chart with total assets sum
fetchJson('/Home/GetAssetsChart').then(data => {
    if (!data) return;
    const totalSum = data.reduce((sum, item) => sum + item.total, 0);

    createChart('SummPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });

    createChart('SummBar', {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: "Portfolio Share %",
                data: data.map(d => ((d.total / totalSum) * 100).toFixed(2)),
            }]
        },
        options: {
            responsive: false,
            maintainAspectRatio: false,
            plugins: {
                datalabels: {
                    anchor: 'end',
                    align: 'top',
                    offset: -2,
                    formatter: v => v + "%",
                    font: { weight: 'bold' },
                    color: '#000'
                }
            }
        }
    });

    const summEl = document.getElementById("Summ");
    if (summEl) summEl.innerHTML = totalSum.toLocaleString("ru-RU", {
        style: "currency", currency: "RUB"
    });
});

// Total SUM  
fetchJson('/Home/GetCurrAssets').then(data => {
    const summEl = document.getElementById("totalCurrSum");
    if (summEl) {
        summEl.innerHTML = data.toLocaleString("ru-RU", {
            style: "currency",
            currency: "RUB"
        });
    }
});


// View current, invested and change sum USD Stocks
document.addEventListener("DOMContentLoaded", async () => {
    try {
        // Current sum
        const currentEl = document.getElementById("currentStocksUSD");
        const currentSum = await fetchJson('/StocksUSD/GetCurrentSUM');

        if (currentEl) {
            currentEl.innerHTML = currentSum.toLocaleString("ru-RU", {
                style: "currency",
                currency: "RUB"
            });
        }
        //

        // Change sum percent 
        const changeSumPercentEl = document.getElementById("changeStocksUSDPercent");
        const changeSumPercent = await fetchJson('/StocksUSD/GetChangePercentageSUM');

        const colorClass = changeSumPercent >= 0 ? "text-success" : "text-danger";

        changeSumPercentEl.textContent = `(${changeSumPercent.toFixed(2)} %)`;
        changeSumPercentEl.classList.add(colorClass);
        //

        // Change Sum
        const changeSumEl = document.getElementById("changeStocksUSD");
        const changeSum = await fetchJson('/StocksUSD/GetChangeSUM');
        const arrow = changeSum >= 0 ? "▲" : "▼";

        changeSumEl.textContent = arrow + " " + changeSum.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";

        changeSumEl.classList.add(colorClass);
        //
    }
    catch (err) {
        console.log("Error loading summ", err);
    }
});


// Pie chart with real estate and transport
fetchJson('/Home/GetETrChart').then(data => {
    if (!data) return;
    const totalSum = data.reduce((sum, item) => sum + item.total, 0);
    createChart('EstateTransportPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });

    createChart('SumEstateTransport', {
        type: 'bar',
        data: {
            labels: data.map(d => d.label),
            datasets: [{
                label: "Portfolio Share %",
                data: data.map(d => ((d.total / totalSum) * 100).toFixed(2)),
            }]
        },
        options: {
            responsive: false,
            maintainAspectRatio: false,
            plugins: {
                datalabels: {
                    anchor: 'end',
                    align: 'top',
                    offset: -2,
                    formatter: v => v + "%",
                    font: { weight: 'bold' },
                    color: '#000'
                }
            }
        }
    });

    const summEl = document.getElementById("SummEstTrans");
    if (summEl) summEl.innerHTML = totalSum.toLocaleString("ru-RU", {
        style: "currency", currency: "RUB"
    });
});

// USD exchange rate
async function getUsdRate() {
    try {
        const rate = await fetchJson('/Home/GetRateContr');
        if (rate != null) {
            const el = document.getElementById("RateUSD");
            if (el) el.innerHTML = rate.toFixed(2) + " ₽";
            return rate;
        }
    } catch (err) {
        console.error("Error getting USD exchange rate:", err);
        return null;
    }
}               //
// Currency autocomplete                // Currency
document.addEventListener("DOMContentLoaded", async () => {
    const nameCurrency = document.getElementById("NameInputCurrency");
    if (!nameCurrency) return;

    try {
        const json = await fetchJson("https://www.cbr-xml-daily.ru/daily_json.js");
        const valutes = json.Valute;

        const names = Object.values(valutes).map(v => v.Name);

        $(nameCurrency).autocomplete({
            source: names,
            minLength: 1
        });
    } catch (error) {
        console.error("Error", error);
    }
});                //

// Startups purchase pie chart         
fetchJson('/Startups/GetChartT').then(data => {
    if (!data) return;
    createChart('StartupsPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});

// Platforms purchase pie chart         
fetchJson('/PlatformStup/GetChartT').then(data => {
    if (!data) return;
    createChart('PlatformsPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});

// Platforms pie chart by number of companies       
fetchJson('/PlatformStup/GetChartCountComp').then(data => {
    if (!data) return;
    createChart('PlatformsCount', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});

// Startups pie chart by number of companies       
fetchJson('/Startups/GetChartCountComp').then(data => {
    if (!data) return;
    createChart('StartupCountStocks', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});

// Cities list
document.addEventListener("DOMContentLoaded", () => {
    const cityInput = document.getElementById("CityInput");
    if (!cityInput) return;

    $(cityInput).autocomplete({
        source: async (request, response) => {
            $.ajax({
                url: "/RealEstate/ListCities",
                data: { term: request.term },
                success: function (data) {
                    response(data);
                },
                error: function (err) {
                    console.error("Error: ", err);
                }
            });
        },
        minLength: 1,
        delay: 200
    });

});

// Transport pie chart by transport type     
fetchJson('/Transport/GetChartTTrans').then(data => {
    if (!data) return;
    createChart('TypeTransport', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});
// Transport pie chart by transport sum       
fetchJson('/Transport/GetChartSTrans').then(data => {
    if (!data) return;
    createChart('SumOfTransport', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});


// Real estate pie chart by cities     
fetchJson('/RealEstate/GetChartC').then(data => {
    if (!data) return;
    createChart('CitiesRealEstates', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});
// Real estate pie chart by estate type       
fetchJson('/RealEstate/GetChartT').then(data => {
    if (!data) return;
    createChart('TypeRealEstate', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});


// Current prices and currency chart           //
document.addEventListener("DOMContentLoaded", () => {

    const chartDataCurrent = [];    // For chart building
    const rows = document.querySelectorAll("tr[data-currency]");  // for chart
    let count = 0;  // Counter for chart building

    let totalCurrSum = 0;
    const el = document.getElementById("totalCurrSum");
    const investedSum = parseFloat(
        document.getElementById("investedSum").dataset.value
    ) || 0;

    document.querySelectorAll("tr[data-currency]").forEach(row => {

        const symbol = row.getAttribute("data-currency");    // this block declares variables
        const currPriceCell = row.querySelector(".current-price");  // for calculating changes
        const changePriceCell = row.querySelector(".change-price");     // in portfolio
        const changeSumRUBCell = row.querySelector(".change-sumRUB");
        const currentSumRUBCell = row.querySelector(".current-sumRUB");
        const nameCurrency = row.querySelector(".nameCurrency");    // for current currency price chart
        if (!symbol || !currPriceCell) return;


        const amountCurrency = parseFloat(row.cells[5].textContent.replace(",", ".").trim()); // extract quantity for calculating sum change
        const purchaseCurrency = parseFloat(row.cells[2].textContent.replace(",", ".").trim()); // Currency purchase price
        const sumPurchase = parseFloat(row.cells[6].textContent.replace(",", ".").trim()); // Currency purchase sum
        let decimals = 2;   // For number precision
        fetch(`/Currency/PriceCurrency?symbol=${encodeURIComponent(symbol)}`)
            .then(res => res.text())    // Get current price of specific currency
            .then(price => {

                const currentPrice = parseFloat(price);
                currPriceCell.textContent = parseFloat(currentPrice).toFixed(decimals) + " ₽"; // Current currency price                
                const changePercent = ((currentPrice - purchaseCurrency) / purchaseCurrency) * 100;  // Change in percent
                const changeFormatPrice = (currentPrice - purchaseCurrency).toFixed(2) + " ₽" + " (" + changePercent.toFixed(2) + " %)";
                // Price change
                changePriceCell.textContent = changeFormatPrice;
                changePriceCell.style.color = changePercent >= 0 ? "green" : "red";   // Change sum color

                const currentSum = currentPrice * amountCurrency; // Current currency value
                currentSumRUBCell.textContent = currentSum.toFixed(decimals) + " ₽";

                const changeFormatSum = (currentSum - sumPurchase).toFixed(2) + " ₽" + " (" + changePercent.toFixed(2) + " %)";
                // Price change

                changeSumRUBCell.textContent = changeFormatSum;
                changeSumRUBCell.style.color = changePercent >= 0 ? "green" : "red";   // Change sum color
                totalCurrSum += currentSum;
                chartDataCurrent.push({ label: nameCurrency.textContent, value: currentSum });
            })
            .catch(() => {  // Catch error
                currPriceCell.textContent = "error";
            })
            .finally(() => {    // When row processed, check if we can build chart
                count++;
                if (count === rows.length) {

                    const changeValue = totalCurrSum - investedSum; // For show 
                    const changePercent = investedSum !== 0 //change Total Sum
                        ? (changeValue / investedSum) * 100
                        : 0;
                    const changeEl = document.getElementById("totalChange");
                    const percentEl = document.getElementById("totalChangePercent");
                    // Форматирование
                    const arrow = changeValue >= 0 ? "▲" : "▼";
                    const colorClass = changeValue >= 0 ? "text-success" : "text-danger";

                    changeEl.textContent = arrow + " " + changeValue.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";
                    percentEl.textContent = `(${changePercent.toFixed(2)} %)`;

                    changeEl.classList.add(colorClass);
                    percentEl.classList.add(colorClass);

                    el.innerHTML = totalCurrSum.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";

                    createChart('CurrentCurrencyPie', {
                        type: 'pie',
                        data: {
                            labels: chartDataCurrent.map(d => d.label),
                            datasets: [{ data: chartDataCurrent.map(d => d.value) }]
                        },
                        options: { responsive: true, maintainAspectRatio: false }
                    });
                }
            });
    });

});                 //
// Currency pie chart                //
fetchJson('/Currency/GetChartTicker').then(data => {
    if (!data) return;
    createChart('CurrencyPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});    //

// Foreign companies tickers pie chart
fetchJson('/StocksUSD/GetChartT').then(data => {
    if (!data) return;
    createChart('TickerPieUSD', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});     //
// Current stock value in rubles              // Foreign stocks
document.addEventListener("DOMContentLoaded", async () => {

    const rateUSD = await getUsdRate(); // For USD exchange rate and current stock value
    const chartDataCurrent = [];    // For chart building
    const rows = document.querySelectorAll("tr[data-stocksUSD]");  // for chart
    let count = 0;  // Counter for chart building    

    rows.forEach(row => {
        const tickerStock = row.getAttribute("data-stocksUSD");
        const changeSumRUBCell = row.querySelector(".change-sumRUB");   // Variables for 
        const currentSumRUBCell = row.querySelector(".current-sumRUB"); // calculating portfolio changes

        if (!changeSumRUBCell || !currentSumRUBCell) return;
        const sumStockUSD = parseFloat(row.cells[4].textContent.replace("$", "").replace(",", ".").trim()); // Extract stock purchase cost
        const sumStockRUB = parseFloat(row.cells[5].textContent.replace("₽", "").replace(",", ".").trim()); // extract quantity for calculating sum change

        const currentSumRUB = sumStockUSD * rateUSD; // Current value in rubles
        currentSumRUBCell.textContent = currentSumRUB.toFixed(2) + " ₽";

        const changePercentSumRUB = ((currentSumRUB - sumStockRUB) / sumStockRUB) * 100;  // Change in percent
        const changeFormatSumRUB = (currentSumRUB - sumStockRUB).toFixed(2) + " ₽" + " (" + changePercentSumRUB.toFixed(2) + " %)";
        changeSumRUBCell.textContent = changeFormatSumRUB;
        changeSumRUBCell.style.color = changePercentSumRUB >= 0 ? "green" : "red";   // Change sum color      

        chartDataCurrent.push({ label: tickerStock, value: currentSumRUB });    // Add data for chart
        count++;

        if (count === rows.length) {    // if all stocks processed, build chart          

            createChart('CurrentStocksUSDPie', {
                type: 'pie',
                data: {
                    labels: chartDataCurrent.map(d => d.label),
                    datasets: [{ data: chartDataCurrent.map(d => d.value) }]
                },
                options: { responsive: true, maintainAspectRatio: false }
            });
        }
    });
});                  //


// Crypto pie chart
fetchJson('/Crypto/GetChartTicker').then(data => {
    if (!data) return;
    createChart('CryptoPie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});    //
// Cryptocurrency tickers list                    //
document.addEventListener("DOMContentLoaded", async () => {
    const tickerInput = document.getElementById("TickerInput");
    if (!tickerInput) return;

    const json = await fetchJson("https://api.bybit.com/v5/market/tickers?category=spot");
    if (json && json.retCode === 0 && json.result?.list) {
        const tickers = json.result.list.map(x => x.symbol);
        $(tickerInput).autocomplete({ source: tickers, minLength: 1 });
    }
});                   // Cryptocurrency
// Current cryptocurrency prices                          //
document.addEventListener("DOMContentLoaded", async () => {

    const rateUSD = await getUsdRate(); // For USD exchange rate and current cryptocurrency value
    const chartDataCurrent = [];    // For chart building
    const rows = document.querySelectorAll("tr[data-cryptos]");  // for chart
    let count = 0;  // Counter for chart building

    let totalCurrSum = 0;
    const el = document.getElementById("totalCurrSum");
    const investedSum = parseFloat(
        document.getElementById("investedSum").dataset.value
    ) || 0;

    document.querySelectorAll("tr[data-cryptos]").forEach(row => {

        const symbol = row.getAttribute("data-cryptos");              // this block declares variables
        const currPriceCell = row.querySelector(".current-price");    // for calculating changes
        const changeSumCell = row.querySelector(".change-sum");       // in portfolio
        const currentSumCell = row.querySelector(".current-sum");
        const changePriceCell = row.querySelector(".change-price");
        const changeSumRUBCell = row.querySelector(".change-sumRUB");
        const currentSumRUBCell = row.querySelector(".current-sumRUB");

        if (!symbol || !currPriceCell || !changePriceCell) return;

        const purchasePrice = parseFloat(row.cells[2].textContent.replace("$", "")
            .replace(",", ".").trim()); // extract purchase price and replace comma with dot
        const amountCrypto = parseFloat(row.cells[5].textContent.replace("$", "").replace(",", ".").trim()); // extract quantity for calculating sum change
        const sumRUB = parseFloat(row.cells[9].textContent.replace("₽", "").replace(",", ".").trim()); // extract quantity for calculating sum change

        let decimals;   // For number precision
        if (purchasePrice >= 1) decimals = 2;
        else if (purchasePrice >= 0.01) decimals = 3;
        else decimals = 7;

        fetch(`/Crypto/PriceCrypto?symbol=${encodeURIComponent(symbol)}`)
            .then(res => res.json())
            .then(price => {
                const sumCrypto = parseFloat(row.cells[6].textContent.replace("$", "").replace(",", ".").trim()); // Extract purchase sum and replace comma with dot
                row.cells[2].textContent = purchasePrice.toFixed(decimals) + " $";
                if (!price || isNaN(price)) {   // If cryptocurrency ticker not found on Bybit, show data below
                    currPriceCell.textContent = "no data";
                    changePriceCell.textContent = "-";
                    changeSumCell.textContent = "-";
                    currentSumCell.textContent = sumCrypto.toFixed(decimals) + " $";
                    changeSumRUBCell.textContent = "-";

                    const currentSum = purchasePrice * amountCrypto; // Current cryptocurrency value
                    const currentSumRUB = currentSum * rateUSD; // Current value in rubles
                    currentSumRUBCell.textContent = currentSumRUB.toFixed(2) + " ₽";

                    const changePercentSumRUB = ((currentSumRUB - sumRUB) / sumRUB) * 100;  // Change in percent
                    const changeFormatSumRUB = (currentSumRUB - sumRUB).toFixed(2) + " ₽" + " (" + changePercentSumRUB.toFixed(2) + " %)";
                    changeSumRUBCell.textContent = changeFormatSumRUB;
                    changeSumRUBCell.style.color = changePercentSumRUB >= 0 ? "green" : "red";   // Change sum color 
                    totalCurrSum += currentSumRUB;
                    chartDataCurrent.push({ label: symbol, value: currentSumRUB });
                }
                else {
                    const currentPrice = parseFloat(price);
                    currPriceCell.textContent = parseFloat(price).toFixed(2) + " $";     // Current cryptocurrency price  

                    const changePercent = ((currentPrice - purchasePrice) / purchasePrice) * 100;  // Change in percent
                    const changeFormatPrice = (currentPrice - purchasePrice).toFixed(decimals) + " $" + " (" + changePercent.toFixed(2) + " %)";   // For displaying price change and percentage
                    changePriceCell.textContent = changeFormatPrice;    // Change in dollars
                    changePriceCell.style.color = changePercent >= 0 ? "green" : "red"; // Percentage color              

                    // Change in value 
                    const currentSum = currentPrice * amountCrypto; // Current cryptocurrency value
                    currentSumCell.textContent = currentSum.toFixed(decimals) + " $";
                    const changeFormatSum = (currentSum - sumCrypto).toFixed(2) + " $" + " (" + changePercent.toFixed(2) + " %)";
                    changeSumCell.textContent = changeFormatSum;
                    changeSumCell.style.color = changePercent >= 0 ? "green" : "red";   // Change sum color

                    // Change in value in rubles
                    const currentSumRUB = currentSum * rateUSD; // Current value in rubles
                    currentSumRUBCell.textContent = currentSumRUB.toFixed(2) + " ₽";

                    const changePercentSumRUB = ((currentSumRUB - sumRUB) / sumRUB) * 100;  // Change in percent
                    const changeFormatSumRUB = (currentSumRUB - sumRUB).toFixed(2) + " ₽" + " (" + changePercentSumRUB.toFixed(2) + " %)";
                    changeSumRUBCell.textContent = changeFormatSumRUB;
                    changeSumRUBCell.style.color = changePercentSumRUB >= 0 ? "green" : "red";   // Change sum color

                    chartDataCurrent.push({ label: symbol, value: currentSumRUB });
                }
            })
            .catch(() => {
                currPriceCell.textContent = "error";
                changePriceCell.textContent = "-";
            })
            .finally(() => {    // When row processed, check if we can build chart

                count++;
                if (count === rows.length) {

                    const changeValue = totalCurrSum - investedSum; // For show 
                    const changePercent = investedSum !== 0 //change Total Sum
                        ? (changeValue / investedSum) * 100
                        : 0;
                    const changeEl = document.getElementById("totalChange");
                    const percentEl = document.getElementById("totalChangePercent");
                    // Форматирование
                    const arrow = changeValue >= 0 ? "▲" : "▼";
                    const colorClass = changeValue >= 0 ? "text-success" : "text-danger";

                    changeEl.textContent = arrow + " " + changeValue.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";
                    percentEl.textContent = `(${changePercent.toFixed(2)} %)`;

                    changeEl.classList.add(colorClass);
                    percentEl.classList.add(colorClass);

                    el.innerHTML = totalCurrSum.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";

                    createChart('CurrentCryptoPie', {
                        type: 'pie',
                        data: {
                            labels: chartDataCurrent.map(d => d.label),
                            datasets: [{ data: chartDataCurrent.map(d => d.value) }]
                        },
                        options: { responsive: true, maintainAspectRatio: false }
                    });
                }
            });

    });
});                  //


// Current prices and metals chart            //
document.addEventListener("DOMContentLoaded", () => {

    const chartDataCurrent = [];    // For chart building
    const rows = document.querySelectorAll("tr[data-metals]");  // for chart
    let count = 0;  // Counter for chart building
    let totalCurrSum = 0;
    const el = document.getElementById("totalCurrSum");
    const investedSum = parseFloat(
        document.getElementById("investedSum").dataset.value
    ) || 0;
    document.querySelectorAll("tr[data-metals]").forEach(row => {

        const symbol = row.getAttribute("data-metals");             // this block declares variables
        const currPriceCell = row.querySelector(".current-price");  // for calculating changes
        const changeSumCell = row.querySelector(".change-sum");     // in portfolio
        const currentSumCell = row.querySelector(".current-sum");

        if (!symbol || !currPriceCell) return;


        const amountMetal = parseFloat(row.cells[3].textContent.replace(",", ".").trim()); // extract quantity for calculating sum change
        let decimals = 2;   // For number precision

        fetch(`/Metals/PriceMetal?nameMetal=${encodeURIComponent(symbol)}`)
            .then(res => res.text())    // Get current price of specific metal
            .then(price => {
                //console.log(price);
                const sumMetal = parseFloat(row.cells[4].textContent.replace("₽", "").replace(",", ".").trim()); // Extract purchase sum and replace comma with dot

                const currentPrice = parseFloat(price.replace(",", "."));
                currPriceCell.textContent = parseFloat(currentPrice).toFixed(decimals) + " ₽"; // Current metal price             

                // Change in value 
                const currentSum = currentPrice * amountMetal; // Current metal value
                currentSumCell.textContent = currentSum.toFixed(decimals) + " ₽";
                const changePercent = ((currentSum - sumMetal) / sumMetal) * 100;  // Change in percent
                const changeFormatSum = (currentSum - sumMetal).toFixed(2) + " ₽" + " (" + changePercent.toFixed(2) + " %)";
                changeSumCell.textContent = changeFormatSum;
                changeSumCell.style.color = changePercent >= 0 ? "green" : "red";   // Change sum color
                totalCurrSum += currentSum;
                chartDataCurrent.push({ label: symbol, value: currentSum });
            })
            .catch(() => {  // Catch error
                currPriceCell.textContent = "error";
            })
            .finally(() => {    // When row processed, check if we can build chart
                count++;
                if (count === rows.length) {
                    const changeValue = totalCurrSum - investedSum; // For show 
                    const changePercent = investedSum !== 0 //change Total Sum
                        ? (changeValue / investedSum) * 100
                        : 0;
                    const changeEl = document.getElementById("totalChange");
                    const percentEl = document.getElementById("totalChangePercent");
                    // Форматирование
                    const arrow = changeValue >= 0 ? "▲" : "▼";
                    const colorClass = changeValue >= 0 ? "text-success" : "text-danger";

                    changeEl.textContent = arrow + " " + changeValue.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";
                    percentEl.textContent =`(${changePercent.toFixed(2)} %)`;

                    changeEl.classList.add(colorClass);
                    percentEl.classList.add(colorClass);

                    el.innerHTML = totalCurrSum.toLocaleString('ru-RU', { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " ₽";

                    createChart('CurrentPricePie', {
                        type: 'pie',
                        data: {
                            labels: chartDataCurrent.map(d => d.label),
                            datasets: [{ data: chartDataCurrent.map(d => d.value) }]
                        },
                        options: { responsive: true, maintainAspectRatio: false }
                    });
                }
            });
        

    });
    
});                 // Metals 
// Metals purchase pie chart         //
fetchJson('/Metals/GetChartT').then(data => {
    if (!data) return;
    createChart('PurchasePie', {
        type: 'pie',
        data: {
            labels: data.map(d => d.label),
            datasets: [{ data: data.map(d => d.total) }]
        },
        options: { responsive: false, maintainAspectRatio: false }
    });
});      //
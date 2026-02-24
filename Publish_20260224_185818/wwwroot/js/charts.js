// Chart.js interop for Blazor
window.chartInterop = {
    _instances: {},

    create: function (canvasId, config) {
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        // Destroy existing chart on same canvas
        if (this._instances[canvasId]) {
            this._instances[canvasId].destroy();
        }

        this._instances[canvasId] = new Chart(canvas, config);
    },

    destroy: function (canvasId) {
        if (this._instances[canvasId]) {
            this._instances[canvasId].destroy();
            delete this._instances[canvasId];
        }
    },

    createDoughnut: function (canvasId, labels, data, colors) {
        this.create(canvasId, {
            type: 'doughnut',
            data: {
                labels: labels,
                datasets: [{
                    data: data,
                    backgroundColor: colors,
                    borderWidth: 2,
                    borderColor: '#fff',
                    hoverOffset: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '65%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: {
                            padding: 16,
                            usePointStyle: true,
                            pointStyleWidth: 10,
                            font: { size: 12, family: 'Inter, Roboto, sans-serif' }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        titleFont: { family: 'Inter, Roboto, sans-serif' },
                        bodyFont: { family: 'Inter, Roboto, sans-serif' },
                        padding: 10,
                        cornerRadius: 8
                    }
                }
            }
        });
    },

    createBar: function (canvasId, labels, datasets, stacked) {
        this.create(canvasId, {
            type: 'bar',
            data: {
                labels: labels,
                datasets: datasets.map(function (ds) {
                    return {
                        label: ds.label,
                        data: ds.data,
                        backgroundColor: ds.color,
                        borderRadius: 4,
                        borderSkipped: false,
                        maxBarThickness: 40
                    };
                })
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        stacked: stacked || false,
                        grid: { display: false },
                        ticks: { font: { size: 11, family: 'Inter, Roboto, sans-serif' } }
                    },
                    y: {
                        stacked: stacked || false,
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.06)' },
                        ticks: { font: { size: 11, family: 'Inter, Roboto, sans-serif' } }
                    }
                },
                plugins: {
                    legend: {
                        display: datasets.length > 1,
                        position: 'top',
                        labels: {
                            padding: 16,
                            usePointStyle: true,
                            font: { size: 12, family: 'Inter, Roboto, sans-serif' }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        padding: 10,
                        cornerRadius: 8
                    }
                }
            }
        });
    },

    createLine: function (canvasId, labels, datasets) {
        this.create(canvasId, {
            type: 'line',
            data: {
                labels: labels,
                datasets: datasets.map(function (ds) {
                    return {
                        label: ds.label,
                        data: ds.data,
                        borderColor: ds.color,
                        backgroundColor: ds.color + '20',
                        fill: true,
                        tension: 0.3,
                        pointRadius: 3,
                        pointHoverRadius: 6,
                        borderWidth: 2
                    };
                })
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                scales: {
                    x: {
                        grid: { display: false },
                        ticks: { font: { size: 11, family: 'Inter, Roboto, sans-serif' } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.06)' },
                        ticks: { font: { size: 11, family: 'Inter, Roboto, sans-serif' } }
                    }
                },
                plugins: {
                    legend: {
                        display: datasets.length > 1,
                        position: 'top',
                        labels: {
                            padding: 16,
                            usePointStyle: true,
                            font: { size: 12, family: 'Inter, Roboto, sans-serif' }
                        }
                    },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.9)',
                        mode: 'index',
                        intersect: false,
                        padding: 10,
                        cornerRadius: 8
                    }
                }
            }
        });
    }
};

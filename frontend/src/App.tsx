import { useState, useEffect } from 'react'
import { BenchmarkResult, ComparisonData } from './types'
import Dashboard from './components/Dashboard'
import SummaryCards from './components/SummaryCards'
import LatencyChart from './components/LatencyChart'
import ThroughputChart from './components/ThroughputChart'
import MemoryChart from './components/MemoryChart'
import OperationFilter from './components/OperationFilter'
import DatasetFilter from './components/DatasetFilter'
import './App.css'

function App() {
  const [results, setResults] = useState<BenchmarkResult[]>([])
  const [filteredResults, setFilteredResults] = useState<BenchmarkResult[]>([])
  const [selectedOperation, setSelectedOperation] = useState<string>('ALL')
  const [selectedDataset, setSelectedDataset] = useState<number>(0)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    loadResults()
  }, [])

  useEffect(() => {
    filterResults()
  }, [results, selectedOperation, selectedDataset])

  const loadResults = async () => {
    try {
      // Try to load from public folder first (for demo)
      const response = await fetch('/benchmark-results.json')
      if (response.ok) {
        const data = await response.json()
        setResults(data)
      } else {
        // Fallback: allow file upload
        console.log('No default results found. Use file upload to load results.')
      }
    } catch (error) {
      console.error('Error loading results:', error)
    } finally {
      setLoading(false)
    }
  }

  const handleFileUpload = (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    if (file) {
      const reader = new FileReader()
      reader.onload = (e) => {
        try {
          const data = JSON.parse(e.target?.result as string)
          setResults(data)
        } catch (error) {
          alert('Invalid JSON file')
        }
      }
      reader.readAsText(file)
    }
  }

  const filterResults = () => {
    let filtered = results

    if (selectedOperation !== 'ALL') {
      filtered = filtered.filter(r => r.operation === selectedOperation)
    }

    if (selectedDataset > 0) {
      filtered = filtered.filter(r => r.datasetSize === selectedDataset)
    }

    setFilteredResults(filtered)
  }

  const getComparisons = (): ComparisonData[] => {
    const comparisons: ComparisonData[] = []
    const operations = Array.from(new Set(filteredResults.map(r => r.operation)))
    const datasets = Array.from(new Set(filteredResults.map(r => r.datasetSize)))

    for (const operation of operations) {
      for (const dataset of datasets) {
        const jit = filteredResults.find(r => r.apiType === 'JIT' && r.operation === operation && r.datasetSize === dataset)
        const aot = filteredResults.find(r => r.apiType === 'AOT' && r.operation === operation && r.datasetSize === dataset)

        if (jit && aot) {
          comparisons.push({
            jit,
            aot,
            improvement: {
              avgLatency: ((jit.avgLatencyMs - aot.avgLatencyMs) / jit.avgLatencyMs) * 100,
              p99: ((jit.p99 - aot.p99) / jit.p99) * 100,
              throughput: ((aot.throughputRps - jit.throughputRps) / jit.throughputRps) * 100,
              memory: ((jit.memoryMb - aot.memoryMb) / jit.memoryMb) * 100
            }
          })
        }
      }
    }

    return comparisons
  }

  if (loading) {
    return <div className="loading">Loading benchmark results...</div>
  }

  const comparisons = getComparisons()

  return (
    <div className="app">
      <header className="app-header">
        <h1>.NET 10 Native AOT vs JIT Benchmark Dashboard</h1>
        <div className="file-upload">
          <input type="file" accept=".json" onChange={handleFileUpload} />
          <span>Upload benchmark results JSON</span>
        </div>
      </header>

      <div className="filters">
        <OperationFilter
          operations={Array.from(new Set(results.map(r => r.operation)))}
          selected={selectedOperation}
          onChange={setSelectedOperation}
        />
        <DatasetFilter
          datasets={Array.from(new Set(results.map(r => r.datasetSize))).sort((a, b) => a - b)}
          selected={selectedDataset}
          onChange={setSelectedDataset}
        />
      </div>

      {comparisons.length > 0 && (
        <>
          <SummaryCards comparisons={comparisons} />
          <Dashboard comparisons={comparisons} />
        </>
      )}

      {filteredResults.length === 0 && (
        <div className="no-data">
          <p>No benchmark results found. Please upload a JSON file with benchmark results.</p>
        </div>
      )}
    </div>
  )
}

export default App


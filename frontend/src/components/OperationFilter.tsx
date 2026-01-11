import './Filter.css'

interface OperationFilterProps {
  operations: string[]
  selected: string
  onChange: (operation: string) => void
}

export default function OperationFilter({ operations, selected, onChange }: OperationFilterProps) {
  return (
    <div className="filter">
      <label htmlFor="operation-filter">Operation:</label>
      <select
        id="operation-filter"
        value={selected}
        onChange={(e) => onChange(e.target.value)}
      >
        <option value="ALL">All Operations</option>
        {operations.map(op => (
          <option key={op} value={op}>{op}</option>
        ))}
      </select>
    </div>
  )
}


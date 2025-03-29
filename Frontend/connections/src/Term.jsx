import './Term.css'

const Term = ({ termName, isSelected, onClick }) => {
    return (
        <div 
            className={`term ${isSelected ? 'selected' : ''}`}
            onClick={onClick}
        >
            {termName}
        </div>
    );
}

export default Term;
import React from 'react'
//import "../../../../node_modules/react-loader-spinner/dist/loader/css/react-spinner-loader.css"
import { Watch } from 'react-loader-spinner'

const Loading = () => {
    return (
        <div align='center' className="loading-spinner">
            <Watch 
                color="#17B169"
                height={100}
                width={100}
                ariaLabel='loading'
            />
        </div>
    )
}

export default Loading;